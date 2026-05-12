using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public sealed class SpellEditorWindow: EditorWindow
    {
        private SpellEditorAssetIndex assetIndex;
        private SpellEditorBuildResult buildResult;
        private List<SpellEditorIssue> issues = new();
        private int selectedMainSpellId;
        private Vector2 scroll;
        private Vector2 spellTableScroll;
        private Vector2 buffTableScroll;
        private bool needsRebuild;

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
            foreach (SpellEditorIssue issue in this.issues)
            {
                EditorGUILayout.HelpBox(issue.Message, issue.Severity);
            }

            EditorGUILayout.LabelField($"资产索引：Spell {this.assetIndex.Spells.Count} / Buff {this.assetIndex.Buffs.Count}", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"当前链路：Spell {this.buildResult?.Spells.Count ?? 0} / Buff {this.buildResult?.Buffs.Count ?? 0}", EditorStyles.miniBoldLabel);
            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField("SpellConfig 表", EditorStyles.boldLabel);
            this.spellTableScroll = EditorGUILayout.BeginScrollView(this.spellTableScroll, GUILayout.Height(this.position.height * 0.42f));
            this.DrawSpellTable();
            EditorGUILayout.EndScrollView();

            GUILayout.Space(8);
            EditorGUILayout.LabelField("BuffConfig 表", EditorStyles.boldLabel);
            this.buffTableScroll = EditorGUILayout.BeginScrollView(this.buffTableScroll, GUILayout.Height(this.position.height * 0.34f));
            this.DrawBuffTable();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndScrollView();

            if (this.needsRebuild)
            {
                this.needsRebuild = false;
                this.RebuildGraph();
                this.Repaint();
            }
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
            this.issues = this.assetIndex != null && this.buildResult != null
                    ? SpellEditorValidator.Validate(this.assetIndex, this.buildResult)
                    : new List<SpellEditorIssue>();
        }

        private void DrawSpellTable()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Id", GUILayout.Width(70));
            GUILayout.Label("类型", GUILayout.Width(60));
            GUILayout.Label("Desc", GUILayout.Width(180));
            GUILayout.Label("IconName", GUILayout.Width(180));
            GUILayout.Label("CD", GUILayout.Width(70));
            GUILayout.Label("Damage", GUILayout.Width(80));
            GUILayout.Label("BuffId", GUILayout.Width(80));
            GUILayout.Label("Cost", GUILayout.Width(160));
            GUILayout.Label("TargetSelector", GUILayout.Width(180));
            GUILayout.Label("来源", GUILayout.Width(240));
            GUILayout.Label("资产路径", GUILayout.Width(360));
            EditorGUILayout.EndHorizontal();

            if (this.buildResult == null)
            {
                return;
            }

            foreach (SpellEditorSpellRow row in this.buildResult.Spells.ToArray())
            {
                EditorGUILayout.BeginHorizontal();
                if (row.Asset == null)
                {
                    GUILayout.Label(row.Id.ToString(), GUILayout.Width(70));
                    GUILayout.Label("Missing", GUILayout.Width(60));
                    GUILayout.Label($"Missing Spell {row.Id}", GUILayout.Width(180));
                    GUILayout.Label(string.Empty, GUILayout.Width(180));
                    GUILayout.Label(string.Empty, GUILayout.Width(70));
                    GUILayout.Label(string.Empty, GUILayout.Width(80));
                    GUILayout.Label(string.Empty, GUILayout.Width(80));
                    GUILayout.Label(string.Empty, GUILayout.Width(160));
                    GUILayout.Label(string.Empty, GUILayout.Width(180));
                    GUILayout.Label(this.FormatSources(row.Sources), GUILayout.Width(240));
                    GUILayout.Label(string.Empty, GUILayout.Width(360));
                    EditorGUILayout.EndHorizontal();
                    continue;
                }

                SpellConfig config = row.Asset.SpellConfig;
                GUILayout.Label(config.Id.ToString(), GUILayout.Width(70));
                GUILayout.Label(row.IsMain ? "主技能" : "子技能", GUILayout.Width(60));
                EditorGUI.BeginChangeCheck();
                string desc = EditorGUILayout.TextField(config.Desc ?? string.Empty, GUILayout.Width(180));
                string iconName = EditorGUILayout.TextField(config.IconName ?? string.Empty, GUILayout.Width(180));
                int cd = EditorGUILayout.IntField(config.CD, GUILayout.Width(70));
                int damage = EditorGUILayout.IntField(config.DamageMultiplier, GUILayout.Width(80));
                int buffId = EditorGUILayout.IntField(config.BuffId, GUILayout.Width(80));
                GUILayout.Label(this.FormatRoot(config.Cost), GUILayout.Width(160));
                GUILayout.Label(this.FormatRoot(config.TargetSelector), GUILayout.Width(180));
                GUILayout.Label(this.FormatSources(row.Sources), GUILayout.Width(240));
                GUILayout.Label(row.AssetPath, GUILayout.Width(360));
                if (EditorGUI.EndChangeCheck())
                {
                    config.Desc = string.IsNullOrEmpty(desc) ? null : desc;
                    config.IconName = string.IsNullOrEmpty(iconName) ? null : iconName;
                    config.CD = cd;
                    config.DamageMultiplier = damage;
                    config.BuffId = buffId;
                    EditorUtility.SetDirty(row.Asset);
                    this.needsRebuild = true;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawBuffTable()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Id", GUILayout.Width(70));
            GUILayout.Label("Desc", GUILayout.Width(180));
            GUILayout.Label("Duration", GUILayout.Width(80));
            GUILayout.Label("TickTime", GUILayout.Width(80));
            GUILayout.Label("MaxStack", GUILayout.Width(80));
            GUILayout.Label("Stack", GUILayout.Width(70));
            GUILayout.Label("Overlay", GUILayout.Width(120));
            GUILayout.Label("Notice", GUILayout.Width(120));
            GUILayout.Label("Flags", GUILayout.Width(160));
            GUILayout.Label("Effects", GUILayout.Width(180));
            GUILayout.Label("来源", GUILayout.Width(240));
            GUILayout.Label("资产路径", GUILayout.Width(360));
            EditorGUILayout.EndHorizontal();

            if (this.buildResult == null)
            {
                return;
            }

            foreach (SpellEditorBuffRow row in this.buildResult.Buffs.ToArray())
            {
                EditorGUILayout.BeginHorizontal();
                if (row.Asset == null)
                {
                    GUILayout.Label(row.Id.ToString(), GUILayout.Width(70));
                    GUILayout.Label($"Missing Buff {row.Id}", GUILayout.Width(180));
                    GUILayout.Label(string.Empty, GUILayout.Width(80));
                    GUILayout.Label(string.Empty, GUILayout.Width(80));
                    GUILayout.Label(string.Empty, GUILayout.Width(80));
                    GUILayout.Label(string.Empty, GUILayout.Width(70));
                    GUILayout.Label(string.Empty, GUILayout.Width(120));
                    GUILayout.Label(string.Empty, GUILayout.Width(120));
                    GUILayout.Label(string.Empty, GUILayout.Width(160));
                    GUILayout.Label(string.Empty, GUILayout.Width(180));
                    GUILayout.Label(this.FormatSources(row.Sources), GUILayout.Width(240));
                    GUILayout.Label(string.Empty, GUILayout.Width(360));
                    EditorGUILayout.EndHorizontal();
                    continue;
                }

                BuffConfig config = row.Asset.BuffConfig;
                GUILayout.Label(config.Id.ToString(), GUILayout.Width(70));
                EditorGUI.BeginChangeCheck();
                string desc = EditorGUILayout.TextField(config.Desc ?? string.Empty, GUILayout.Width(180));
                int duration = EditorGUILayout.IntField(config.Duration, GUILayout.Width(80));
                int tickTime = EditorGUILayout.IntField(config.TickTime, GUILayout.Width(80));
                int maxStack = EditorGUILayout.IntField(config.MaxStack, GUILayout.Width(80));
                int stack = EditorGUILayout.IntField(config.Stack, GUILayout.Width(70));
                OverLayRuleType overlay = (OverLayRuleType)EditorGUILayout.EnumPopup(config.OverLayRuleType, GUILayout.Width(120));
                NoticeType notice = (NoticeType)EditorGUILayout.EnumPopup(config.NoticeType, GUILayout.Width(120));
                GUILayout.Label(this.FormatFlags(config), GUILayout.Width(160));
                GUILayout.Label(this.FormatEffects(config), GUILayout.Width(180));
                GUILayout.Label(this.FormatSources(row.Sources), GUILayout.Width(240));
                GUILayout.Label(row.AssetPath, GUILayout.Width(360));
                if (EditorGUI.EndChangeCheck())
                {
                    config.Desc = string.IsNullOrEmpty(desc) ? null : desc;
                    config.Duration = duration;
                    config.TickTime = tickTime;
                    config.MaxStack = maxStack;
                    config.Stack = stack;
                    config.OverLayRuleType = overlay;
                    config.NoticeType = notice;
                    EditorUtility.SetDirty(row.Asset);
                    this.needsRebuild = true;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private string FormatRoot(BTRoot root)
        {
            if (root == null)
            {
                return string.Empty;
            }

            int nodeCount = CountNodes(root);
            return $"{root.GetType().Name} ({nodeCount})";
        }

        private string FormatEffects(BuffConfig config)
        {
            if (config.Effects == null || config.Effects.Count == 0)
            {
                return string.Empty;
            }

            return $"{config.Effects.Count}: {string.Join(", ", config.Effects.Select(x => x != null? x.GetType().Name : "null"))}";
        }

        private string FormatFlags(BuffConfig config)
        {
            return config.Flags == null || config.Flags.Count == 0
                    ? string.Empty
                    : string.Join(", ", config.Flags.Select(x => x.ToString()));
        }

        private string FormatSources(List<SpellEditorSourceInfo> sources)
        {
            return sources == null || sources.Count == 0 ? string.Empty : string.Join("; ", sources.Select(x => x.ToString()));
        }

        private static int CountNodes(BTNode node)
        {
            if (node == null)
            {
                return 0;
            }

            int count = 1;
            if (node.Children == null)
            {
                return count;
            }

            foreach (BTNode child in node.Children)
            {
                count += CountNodes(child);
            }

            return count;
        }
    }
}
