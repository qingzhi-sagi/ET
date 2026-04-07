#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ET.Client
{
    public static class BTAssetOdinUnityObjectMigrationEditor
    {
        [MenuItem("ET/BehaviorTree/Migrate OdinUnityObject To String")]
        public static void Migrate()
        {
            int migratedAssetCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[]
            {
                "Packages/cn.etetet.statesync/Assets/BT"
            });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                if (asset == null)
                {
                    continue;
                }

                bool dirty = false;
                if (asset is SpellScriptableObject spellAsset)
                {
                    dirty |= MigrateObjectGraph(spellAsset.SpellConfig, new HashSet<object>());
                }

                if (asset is BuffScriptableObject buffAsset)
                {
                    dirty |= MigrateObjectGraph(buffAsset.BuffConfig, new HashSet<object>());
                }

                if (dirty)
                {
                    EditorUtility.SetDirty(asset);
                    migratedAssetCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ExportScriptableObjectEditor.ExportScriptableObject();
            Debug.Log($"BT OdinUnityObject migration complete. Migrated assets: {migratedAssetCount}");
        }

        private static bool MigrateObjectGraph(object root, HashSet<object> visited)
        {
            if (root == null)
            {
                return false;
            }

            Type type = root.GetType();
            if (!type.IsValueType)
            {
                if (visited.Contains(root))
                {
                    return false;
                }

                visited.Add(root);
            }

            bool dirty = false;
            dirty |= MigrateLegacyName(root, "legacyIcon", "IconName", "editorIconName");
            dirty |= MigrateLegacyName(root, "legacySpellIndicator", "SpellIndicatorName");
            dirty |= MigrateLegacyName(root, "legacyEffect", "EffectName");

            FieldInfo editorShowDisplayName = GetField(type, "editorShowDisplayName");
            FieldInfo showDisplayName = GetField(type, "ShowDisplayName");
            if (editorShowDisplayName != null && showDisplayName != null)
            {
                string current = (string)showDisplayName.GetValue(root);
                string editorCurrent = (string)editorShowDisplayName.GetValue(root);
                if (!string.Equals(current, editorCurrent, System.StringComparison.Ordinal))
                {
                    editorShowDisplayName.SetValue(root, current);
                    dirty = true;
                }
            }

            foreach (FieldInfo field in GetFields(type))
            {
                object value = field.GetValue(root);
                if (value == null)
                {
                    continue;
                }

                if (value is string)
                {
                    continue;
                }

                if (value is IEnumerable && !(value is IDictionary))
                {
                    foreach (object item in (IEnumerable)value)
                    {
                        dirty |= MigrateObjectGraph(item, visited);
                    }
                    continue;
                }

                if (field.FieldType.IsPrimitive || field.FieldType.IsEnum)
                {
                    continue;
                }

                dirty |= MigrateObjectGraph(value, visited);
            }

            return dirty;
        }

        private static bool MigrateLegacyName(object obj, string legacyFieldName, string targetFieldName, string editorTargetFieldName = null)
        {
            FieldInfo legacyField = GetField(obj.GetType(), legacyFieldName);
            FieldInfo targetField = GetField(obj.GetType(), targetFieldName);
            if (legacyField == null || targetField == null)
            {
                return false;
            }

            object legacyObject = legacyField.GetValue(obj);
            if (legacyObject == null)
            {
                return false;
            }

            FieldInfo nameField = GetField(legacyObject.GetType(), "Name");
            if (nameField == null)
            {
                return false;
            }

            string legacyName = nameField.GetValue(legacyObject) as string;
            if (string.IsNullOrEmpty(legacyName))
            {
                return false;
            }

            bool dirty = false;
            string currentValue = targetField.GetValue(obj) as string;
            if (!string.Equals(currentValue, legacyName, System.StringComparison.Ordinal))
            {
                targetField.SetValue(obj, legacyName);
                dirty = true;
            }

            if (!string.IsNullOrEmpty(editorTargetFieldName))
            {
                FieldInfo editorTargetField = GetField(obj.GetType(), editorTargetFieldName);
                if (editorTargetField != null)
                {
                    string editorValue = editorTargetField.GetValue(obj) as string;
                    if (!string.Equals(editorValue, legacyName, System.StringComparison.Ordinal))
                    {
                        editorTargetField.SetValue(obj, legacyName);
                        dirty = true;
                    }
                }
            }

            nameField.SetValue(legacyObject, string.Empty);
            return true || dirty;
        }

        private static FieldInfo GetField(Type type, string fieldName)
        {
            while (type != null && type != typeof(object))
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    return field;
                }

                type = type.BaseType;
            }

            return null;
        }

        private static IEnumerable<FieldInfo> GetFields(Type type)
        {
            while (type != null && type != typeof(object))
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (field.IsStatic)
                    {
                        continue;
                    }

                    yield return field;
                }

                type = type.BaseType;
            }
        }
    }
}
#endif
