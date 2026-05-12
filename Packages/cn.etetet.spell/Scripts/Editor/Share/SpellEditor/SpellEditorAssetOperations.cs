using ET.Client;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class SpellEditorAssetOperations
    {
        public static void SaveAssets()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void ExportConfig()
        {
            SaveAssets();
            ExportScriptableObjectEditor.ExportScriptableObject();
        }

        public static void OpenBehaviorTree(Object owner, BTRoot root)
        {
            if (owner == null || root == null)
            {
                return;
            }

            BTNodeDrawer.OpenNode = root;
            BTNodeDrawer.ScriptableObject = owner;
            EditorApplication.ExecuteMenuItem("ET/BehaviorTree/BehaviorTreeEditor");
        }
    }
}
