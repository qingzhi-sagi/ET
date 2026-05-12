using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public sealed class SpellEditorWindow: EditorWindow
    {
        private SpellEditorAssetIndex assetIndex;
        private SpellEditorBuildResult buildResult;
        private int selectedMainSpellId;
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

            this.DrawMainSpellPopup();
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
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Selected Main Spell: {this.selectedMainSpellId}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Chain Spells: {this.buildResult?.Spells.Count ?? 0}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Chain Buffs: {this.buildResult?.Buffs.Count ?? 0}", EditorStyles.boldLabel);

            foreach (int id in this.assetIndex.Spells.Keys.OrderBy(x => x).Take(20))
            {
                EditorGUILayout.LabelField(id.ToString(), this.assetIndex.GetPath(this.assetIndex.Spells[id]));
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawMainSpellPopup()
        {
            if (this.assetIndex == null)
            {
                return;
            }

            int[] mainSpellIds = this.assetIndex.Spells.Keys.Where(SpellEditorConstants.IsMainSpell).OrderBy(x => x).ToArray();
            if (mainSpellIds.Length == 0)
            {
                GUILayout.Label("无主技能", EditorStyles.toolbarButton, GUILayout.Width(120));
                return;
            }

            string[] mainSpellLabels = mainSpellIds.Select(x => x.ToString()).ToArray();
            int currentIndex = Array.IndexOf(mainSpellIds, this.selectedMainSpellId);
            int nextIndex = EditorGUILayout.Popup(Math.Max(0, currentIndex), mainSpellLabels, EditorStyles.toolbarPopup, GUILayout.Width(160));
            if (nextIndex >= 0 && nextIndex < mainSpellIds.Length && mainSpellIds[nextIndex] != this.selectedMainSpellId)
            {
                this.selectedMainSpellId = mainSpellIds[nextIndex];
                this.RebuildGraph();
            }
        }

        private void RefreshIndex()
        {
            this.assetIndex = SpellEditorAssetIndex.Build();
            int firstMain = this.assetIndex.Spells.Keys.Where(SpellEditorConstants.IsMainSpell).OrderBy(x => x).FirstOrDefault();
            if (this.selectedMainSpellId == 0 || !this.assetIndex.Spells.ContainsKey(this.selectedMainSpellId))
            {
                this.selectedMainSpellId = firstMain;
            }

            this.RebuildGraph();
            this.Repaint();
        }

        private void RebuildGraph()
        {
            this.buildResult = this.assetIndex != null && this.selectedMainSpellId != 0
                    ? SpellEditorGraphBuilder.Build(this.assetIndex, this.selectedMainSpellId)
                    : new SpellEditorBuildResult();
        }
    }
}
