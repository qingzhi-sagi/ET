using System.Collections.Generic;
using ET.Client;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class SpellEditorAssetMigration
    {
        [MenuItem("ET/Spell/Migration/Reserialize Spell Assets")]
        private static void ReserializeSpellAssets()
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { SpellEditorConstants.BtAssetRoot });
            List<string> paths = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SpellScriptableObject asset = AssetDatabase.LoadAssetAtPath<SpellScriptableObject>(path);
                if (asset == null)
                {
                    continue;
                }

                paths.Add(path);
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.ForceReserializeAssets(paths);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Reserialized {paths.Count} Spell assets.");
        }

        [MenuItem("ET/Spell/Migration/Rename Compact Asset Names")]
        private static void RenameCompactAssetNames()
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { SpellEditorConstants.BtAssetRoot });
            int spellRenames = 0;
            int buffRenames = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    if (asset is SpellScriptableObject spellAsset)
                    {
                        if (RenameAssetIfNeeded(spellAsset, SpellEditorConstants.SpellAssetName(spellAsset.SpellConfig.Id)))
                        {
                            spellRenames++;
                        }

                        continue;
                    }

                    if (asset is BuffScriptableObject buffAsset)
                    {
                        if (RenameAssetIfNeeded(buffAsset, SpellEditorConstants.BuffAssetName(buffAsset.BuffConfig.Id)))
                        {
                            buffRenames++;
                        }
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Renamed compact Spell/Buff assets. SpellRenames={spellRenames}, BuffRenames={buffRenames}.");
        }

        private static bool RenameAssetIfNeeded(UnityEngine.Object asset, string targetName)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            if (asset.name == targetName)
            {
                return false;
            }

            string error = AssetDatabase.RenameAsset(path, targetName);
            if (!string.IsNullOrEmpty(error))
            {
                throw new System.InvalidOperationException($"RenameAsset failed for {path} -> {targetName}: {error}");
            }

            EditorUtility.SetDirty(asset);
            return true;
        }
    }
}
