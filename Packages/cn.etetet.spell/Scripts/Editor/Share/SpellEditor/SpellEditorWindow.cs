using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public sealed class SpellEditorWindow: EditorWindow
    {
        private SpellEditorAssetIndex assetIndex;
        private Vector2 scroll;

        [MenuItem(SpellEditorConstants.MenuPath)]
        public static void Open()
        {
            SpellEditorWindow window = GetWindow<SpellEditorWindow>();
            window.titleContent = new GUIContent(SpellEditorConstants.WindowTitle);
            window.minSize = new Vector2(960, 540);
            window.Show();
        }

        private void OnEnable()
        {
            this.RefreshIndex();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(64)))
            {
                this.RefreshIndex();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (this.assetIndex == null)
            {
                EditorGUILayout.HelpBox("资产索引未初始化。", MessageType.Warning);
                return;
            }

            this.scroll = EditorGUILayout.BeginScrollView(this.scroll);
            EditorGUILayout.LabelField($"Spell: {this.assetIndex.Spells.Count}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Buff: {this.assetIndex.Buffs.Count}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Duplicate Spell Id: {this.assetIndex.DuplicateSpellPaths.Count}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Duplicate Buff Id: {this.assetIndex.DuplicateBuffPaths.Count}", EditorStyles.boldLabel);

            foreach (int id in this.assetIndex.Spells.Keys.OrderBy(x => x).Take(20))
            {
                EditorGUILayout.LabelField(id.ToString(), this.assetIndex.GetPath(this.assetIndex.Spells[id]));
            }
            EditorGUILayout.EndScrollView();
        }

        private void RefreshIndex()
        {
            this.assetIndex = SpellEditorAssetIndex.Build();
            this.Repaint();
        }
    }
}
