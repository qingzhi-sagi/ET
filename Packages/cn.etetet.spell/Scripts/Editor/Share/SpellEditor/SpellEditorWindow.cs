using UnityEditor;
using UnityEngine;

namespace ET
{
    public sealed class SpellEditorWindow: EditorWindow
    {
        [MenuItem(SpellEditorConstants.MenuPath)]
        public static void Open()
        {
            SpellEditorWindow window = GetWindow<SpellEditorWindow>();
            window.titleContent = new GUIContent(SpellEditorConstants.WindowTitle);
            window.minSize = new Vector2(960, 540);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Spell Editor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Spell/Buff 表格式编辑器入口已创建。后续任务会接入资产扫描、链路构建和表格编辑。", MessageType.Info);
        }
    }
}
