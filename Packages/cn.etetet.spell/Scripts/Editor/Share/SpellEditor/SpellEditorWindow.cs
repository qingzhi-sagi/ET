using System;
using System.Collections.Generic;
using System.Linq;
using ET.Client;
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
        private UnityEngine.Object selectedAsset;
        private int renameId;
        private int copyMainSpellId;
        private readonly Dictionary<int, int> selectedEffectIndexByBuffId = new();
        private Type[] effectNodeTypes;

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

            if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(64)))
            {
                SpellEditorAssetOperations.SaveAssets();
                this.RefreshIndex();
            }

            if (GUILayout.Button("导出配置", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                SpellEditorAssetOperations.ExportConfig();
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

            this.DrawAssetToolbar();

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
                this.copyMainSpellId = SpellEditorAssetOperations.FindNextMainSpellId(this.assetIndex, this.selectedMainSpellId);
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

            this.EnsureSelectionValid();
            if (this.copyMainSpellId == 0 || this.assetIndex.Spells.ContainsKey(this.copyMainSpellId))
            {
                this.copyMainSpellId = SpellEditorAssetOperations.FindNextMainSpellId(this.assetIndex, this.selectedMainSpellId);
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

        private void DrawAssetToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            bool hasMain = this.assetIndex.Spells.TryGetValue(this.selectedMainSpellId, out SpellScriptableObject mainAsset);
            if (GUILayout.Button("新建子技能", EditorStyles.toolbarButton, GUILayout.Width(88)) && hasMain)
            {
                int spellId = SpellEditorAssetOperations.FindNextSubSpellId(this.assetIndex, this.selectedMainSpellId);
                if (spellId == 0)
                {
                    EditorUtility.DisplayDialog("新建失败", $"主技能组 {this.selectedMainSpellId} 没有空闲子技能 Id。", "确定");
                }
                else
                {
                    string directory = SpellEditorAssetOperations.GetAssetDirectory(mainAsset);
                    int buffId = SpellEditorAssetOperations.FindNextBuffId(this.assetIndex, spellId);
                    SpellEditorAssetOperations.CreateSpellAsset(new SpellConfig { Id = spellId, BuffId = buffId }, directory);
                    SpellEditorAssetOperations.CreateBuffAsset(buffId, directory);
                    this.RefreshIndex();
                }
            }

            GUILayout.Space(8);
            if (this.selectedAsset == null)
            {
                GUILayout.Label("未选择资产", EditorStyles.miniLabel, GUILayout.Width(96));
            }
            else
            {
                GUILayout.Label($"选中 {this.selectedAsset.name}", EditorStyles.miniLabel, GUILayout.Width(120));
                GUILayout.Label("目标Id", EditorStyles.miniLabel, GUILayout.Width(44));
                this.renameId = EditorGUILayout.IntField(this.renameId, GUILayout.Width(80));
                if (GUILayout.Button("改Id", EditorStyles.toolbarButton, GUILayout.Width(56)))
                {
                    int oldId = SpellEditorAssetOperations.GetAssetId(this.selectedAsset);
                    bool wasMain = this.selectedAsset is SpellScriptableObject && oldId == this.selectedMainSpellId;
                    if (SpellEditorAssetOperations.RenameAssetId(this.assetIndex, this.selectedAsset, this.renameId))
                    {
                        if (wasMain)
                        {
                            this.selectedMainSpellId = this.renameId;
                        }

                        this.RefreshIndex();
                    }
                }

                if (GUILayout.Button("删除", EditorStyles.toolbarButton, GUILayout.Width(56)))
                {
                    string path = AssetDatabase.GetAssetPath(this.selectedAsset);
                    if (EditorUtility.DisplayDialog("删除资产", $"确认删除 {path}？", "删除", "取消"))
                    {
                        SpellEditorAssetOperations.DeleteAsset(this.selectedAsset);
                        this.SelectAsset(null);
                        this.RefreshIndex();
                    }
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label("复制链到主Id", EditorStyles.miniLabel, GUILayout.Width(84));
            this.copyMainSpellId = EditorGUILayout.IntField(this.copyMainSpellId, GUILayout.Width(80));
            if (GUILayout.Button("复制链", EditorStyles.toolbarButton, GUILayout.Width(64)))
            {
                if (SpellEditorAssetOperations.CopySpellChain(this.assetIndex, this.buildResult, this.selectedMainSpellId, this.copyMainSpellId))
                {
                    this.selectedMainSpellId = this.copyMainSpellId;
                    this.SelectAsset(null);
                    this.RefreshIndex();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSpellTable()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("选", GUILayout.Width(24));
            GUILayout.Label("操作", GUILayout.Width(70));
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
            GUILayout.Label("资产", GUILayout.Width(360));
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
                    GUILayout.Label(string.Empty, GUILayout.Width(24));
                    if (GUILayout.Button("创建", EditorStyles.miniButton, GUILayout.Width(70)))
                    {
                        this.CreateMissingSpell(row.Id);
                        EditorGUILayout.EndHorizontal();
                        continue;
                    }

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
                this.DrawSelectionToggle(row.Asset);
                GUILayout.Label(string.Empty, GUILayout.Width(70));
                GUILayout.Label(config.Id.ToString(), GUILayout.Width(70));
                GUILayout.Label(row.IsMain ? "主技能" : "子技能", GUILayout.Width(60));
                EditorGUI.BeginChangeCheck();
                string desc = EditorGUILayout.TextField(config.Desc ?? string.Empty, GUILayout.Width(180));
                string iconName = EditorGUILayout.TextField(config.IconName ?? string.Empty, GUILayout.Width(180));
                int cd = EditorGUILayout.IntField(config.CD, GUILayout.Width(70));
                int damage = EditorGUILayout.IntField(config.DamageMultiplier, GUILayout.Width(80));
                int buffId = EditorGUILayout.IntField(config.BuffId, GUILayout.Width(80));
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

                if (GUILayout.Button(this.FormatRoot(config.Cost), EditorStyles.miniButton, GUILayout.Width(160)))
                {
                    SpellEditorAssetOperations.OpenBehaviorTree(row.Asset, config.Cost);
                }

                if (GUILayout.Button(this.FormatRoot(config.TargetSelector), EditorStyles.miniButton, GUILayout.Width(180)))
                {
                    SpellEditorAssetOperations.OpenBehaviorTree(row.Asset, config.TargetSelector);
                }

                GUILayout.Label(this.FormatSources(row.Sources), GUILayout.Width(240));
                this.DrawAssetField(row.Asset, GUILayout.Width(360));
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawBuffTable()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("选", GUILayout.Width(24));
            GUILayout.Label("操作", GUILayout.Width(70));
            GUILayout.Label("Id", GUILayout.Width(70));
            GUILayout.Label("Desc", GUILayout.Width(180));
            GUILayout.Label("Duration", GUILayout.Width(80));
            GUILayout.Label("TickTime", GUILayout.Width(80));
            GUILayout.Label("MaxStack", GUILayout.Width(80));
            GUILayout.Label("Stack", GUILayout.Width(70));
            GUILayout.Label("Overlay", GUILayout.Width(120));
            GUILayout.Label("Notice", GUILayout.Width(120));
            GUILayout.Label("Flags", GUILayout.Width(220));
            GUILayout.Label("Effects", GUILayout.Width(360));
            GUILayout.Label("来源", GUILayout.Width(240));
            GUILayout.Label("资产", GUILayout.Width(360));
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
                    GUILayout.Label(string.Empty, GUILayout.Width(24));
                    if (GUILayout.Button("创建", EditorStyles.miniButton, GUILayout.Width(70)))
                    {
                        this.CreateMissingBuff(row.Id);
                        EditorGUILayout.EndHorizontal();
                        continue;
                    }

                    GUILayout.Label(row.Id.ToString(), GUILayout.Width(70));
                    GUILayout.Label($"Missing Buff {row.Id}", GUILayout.Width(180));
                    GUILayout.Label(string.Empty, GUILayout.Width(80));
                    GUILayout.Label(string.Empty, GUILayout.Width(80));
                    GUILayout.Label(string.Empty, GUILayout.Width(80));
                    GUILayout.Label(string.Empty, GUILayout.Width(70));
                    GUILayout.Label(string.Empty, GUILayout.Width(120));
                    GUILayout.Label(string.Empty, GUILayout.Width(120));
                    GUILayout.Label(string.Empty, GUILayout.Width(220));
                    GUILayout.Label(string.Empty, GUILayout.Width(360));
                    GUILayout.Label(this.FormatSources(row.Sources), GUILayout.Width(240));
                    GUILayout.Label(string.Empty, GUILayout.Width(360));
                    EditorGUILayout.EndHorizontal();
                    continue;
                }

                BuffConfig config = row.Asset.BuffConfig;
                this.DrawSelectionToggle(row.Asset);
                GUILayout.Label(string.Empty, GUILayout.Width(70));
                GUILayout.Label(config.Id.ToString(), GUILayout.Width(70));
                EditorGUI.BeginChangeCheck();
                string desc = EditorGUILayout.TextField(config.Desc ?? string.Empty, GUILayout.Width(180));
                int duration = EditorGUILayout.IntField(config.Duration, GUILayout.Width(80));
                int tickTime = EditorGUILayout.IntField(config.TickTime, GUILayout.Width(80));
                int maxStack = EditorGUILayout.IntField(config.MaxStack, GUILayout.Width(80));
                int stack = EditorGUILayout.IntField(config.Stack, GUILayout.Width(70));
                OverLayRuleType overlay = (OverLayRuleType)EditorGUILayout.EnumPopup(config.OverLayRuleType, GUILayout.Width(120));
                NoticeType notice = (NoticeType)EditorGUILayout.EnumPopup(config.NoticeType, GUILayout.Width(120));
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

                this.DrawFlagsCell(row.Asset, config);
                this.DrawEffectsCell(row.Asset, config);
                GUILayout.Label(this.FormatSources(row.Sources), GUILayout.Width(240));
                this.DrawAssetField(row.Asset, GUILayout.Width(360));
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawAssetField(UnityEngine.Object asset, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            EditorGUILayout.ObjectField(asset, asset != null ? asset.GetType() : typeof(UnityEngine.Object), false, GUILayout.Width(300));
            using (new EditorGUI.DisabledScope(asset == null))
            {
                if (GUILayout.Button("定位", EditorStyles.miniButton, GUILayout.Width(52)))
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawFlagsCell(BuffScriptableObject asset, BuffConfig config)
        {
            string label = string.IsNullOrEmpty(this.FormatFlags(config)) ? "无 Flags" : this.FormatFlags(config);
            if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.Width(220)))
            {
                this.ShowFlagsMenu(asset, config);
            }
        }

        private void ShowFlagsMenu(BuffScriptableObject asset, BuffConfig config)
        {
            config.Flags ??= new HashSet<BuffFlags>();
            GenericMenu menu = new();
            foreach (BuffFlags flag in Enum.GetValues(typeof(BuffFlags)).Cast<BuffFlags>().OrderBy(x => (int)x))
            {
                if (flag == BuffFlags.None)
                {
                    continue;
                }

                BuffFlags currentFlag = flag;
                bool selected = config.Flags.Contains(currentFlag);
                menu.AddItem(new GUIContent(currentFlag.ToString()), selected, () => this.ToggleFlag(asset, config, currentFlag));
            }

            menu.AddSeparator(string.Empty);
            if (config.Flags.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("清空"));
            }
            else
            {
                menu.AddItem(new GUIContent("清空"), false, () =>
                {
                    config.Flags.Clear();
                    EditorUtility.SetDirty(asset);
                    this.Repaint();
                });
            }

            menu.ShowAsContext();
        }

        private void ToggleFlag(BuffScriptableObject asset, BuffConfig config, BuffFlags flag)
        {
            config.Flags ??= new HashSet<BuffFlags>();
            if (!config.Flags.Add(flag))
            {
                config.Flags.Remove(flag);
            }

            EditorUtility.SetDirty(asset);
            this.Repaint();
        }

        private void DrawEffectsCell(BuffScriptableObject asset, BuffConfig config)
        {
            config.Effects ??= new List<EffectNode>();
            int effectCount = config.Effects.Count;
            if (effectCount <= 0)
            {
                GUILayout.Label("无 Effect", EditorStyles.miniLabel, GUILayout.Width(220));
                if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(28)))
                {
                    this.ShowAddEffectMenu(asset, config);
                }

                GUILayout.Label(string.Empty, GUILayout.Width(112));
                return;
            }

            int selectedIndex = this.GetSelectedEffectIndex(config.Id, effectCount);
            string[] effectLabels = config.Effects
                    .Select((effect, index) => $"{index}: {(effect != null ? effect.GetType().Name : "null")}")
                    .ToArray();
            int nextIndex = EditorGUILayout.Popup(selectedIndex, effectLabels, GUILayout.Width(220));
            if (nextIndex != selectedIndex)
            {
                this.selectedEffectIndexByBuffId[config.Id] = nextIndex;
                selectedIndex = nextIndex;
            }

            EffectNode selectedEffect = config.Effects[selectedIndex];
            using (new EditorGUI.DisabledScope(selectedEffect == null))
            {
                if (GUILayout.Button("打开", EditorStyles.miniButton, GUILayout.Width(44)))
                {
                    SpellEditorAssetOperations.OpenBehaviorTree(asset, selectedEffect);
                }
            }

            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(28)))
            {
                this.ShowAddEffectMenu(asset, config);
            }

            if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(28)))
            {
                string effectName = selectedEffect != null ? selectedEffect.GetType().Name : "null";
                if (EditorUtility.DisplayDialog("删除 Effect", $"确认删除 Effects[{selectedIndex}] {effectName}？", "删除", "取消"))
                {
                    config.Effects.RemoveAt(selectedIndex);
                    config.effectDict = null;
                    if (config.Effects.Count == 0)
                    {
                        this.selectedEffectIndexByBuffId.Remove(config.Id);
                    }
                    else
                    {
                        this.selectedEffectIndexByBuffId[config.Id] = Mathf.Clamp(selectedIndex, 0, config.Effects.Count - 1);
                    }

                    EditorUtility.SetDirty(asset);
                    this.needsRebuild = true;
                }
            }

            GUILayout.Label($"共 {effectCount}", EditorStyles.miniLabel, GUILayout.Width(40));
        }

        private int GetSelectedEffectIndex(int buffId, int effectCount)
        {
            if (!this.selectedEffectIndexByBuffId.TryGetValue(buffId, out int selectedIndex))
            {
                selectedIndex = 0;
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, effectCount - 1);
            this.selectedEffectIndexByBuffId[buffId] = selectedIndex;
            return selectedIndex;
        }

        private void ShowAddEffectMenu(BuffScriptableObject asset, BuffConfig config)
        {
            GenericMenu menu = new();
            HashSet<Type> existingTypes = config.Effects
                    .Where(effect => effect != null)
                    .Select(effect => effect.GetType())
                    .ToHashSet();

            foreach (Type type in this.GetEffectNodeTypes())
            {
                Type effectType = type;
                GUIContent content = new(effectType.Name);
                if (existingTypes.Contains(effectType))
                {
                    menu.AddDisabledItem(content);
                    continue;
                }

                menu.AddItem(content, false, () => this.AddEffect(asset, config, effectType));
            }

            menu.ShowAsContext();
        }

        private Type[] GetEffectNodeTypes()
        {
            return this.effectNodeTypes ??= TypeCache.GetTypesDerivedFrom<EffectNode>()
                    .Where(type => type is { IsAbstract: false } && !type.ContainsGenericParameters)
                    .OrderBy(type => type.Name)
                    .ToArray();
        }

        private void AddEffect(BuffScriptableObject asset, BuffConfig config, Type effectType)
        {
            if (Activator.CreateInstance(effectType) is not EffectNode effect)
            {
                EditorUtility.DisplayDialog("添加失败", $"无法创建 EffectNode: {effectType.Name}", "确定");
                return;
            }

            config.Effects ??= new List<EffectNode>();
            config.Effects.Add(effect);
            config.effectDict = null;
            this.selectedEffectIndexByBuffId[config.Id] = config.Effects.Count - 1;
            EditorUtility.SetDirty(asset);
            this.needsRebuild = true;
            this.Repaint();
        }

        private void DrawSelectionToggle(UnityEngine.Object asset)
        {
            bool selected = this.selectedAsset == asset;
            bool next = GUILayout.Toggle(selected, string.Empty, GUILayout.Width(24));
            if (next != selected)
            {
                this.SelectAsset(next ? asset : null);
            }
        }

        private void SelectAsset(UnityEngine.Object asset)
        {
            this.selectedAsset = asset;
            this.renameId = SpellEditorAssetOperations.GetAssetId(asset);
            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
            }
        }

        private void EnsureSelectionValid()
        {
            if (this.selectedAsset == null)
            {
                this.renameId = 0;
                return;
            }

            string path = AssetDatabase.GetAssetPath(this.selectedAsset);
            if (string.IsNullOrEmpty(path))
            {
                this.SelectAsset(null);
            }
        }

        private void CreateMissingSpell(int spellId)
        {
            if (!this.assetIndex.Spells.TryGetValue(this.selectedMainSpellId, out SpellScriptableObject mainAsset))
            {
                EditorUtility.DisplayDialog("创建失败", "当前主技能资产不存在。", "确定");
                return;
            }

            string directory = SpellEditorAssetOperations.GetAssetDirectory(mainAsset);
            SpellEditorAssetOperations.CreateSpellAsset(spellId, directory);
            this.RefreshIndex();
        }

        private void CreateMissingBuff(int buffId)
        {
            if (!this.assetIndex.Spells.TryGetValue(this.selectedMainSpellId, out SpellScriptableObject mainAsset))
            {
                EditorUtility.DisplayDialog("创建失败", "当前主技能资产不存在。", "确定");
                return;
            }

            string directory = SpellEditorAssetOperations.GetAssetDirectory(mainAsset);
            SpellEditorAssetOperations.CreateBuffAsset(buffId, directory);
            this.RefreshIndex();
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
                    : string.Join(", ", config.Flags.OrderBy(x => (int)x).Select(x => x.ToString()));
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
