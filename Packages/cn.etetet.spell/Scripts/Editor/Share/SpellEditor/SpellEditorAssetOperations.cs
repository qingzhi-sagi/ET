using System.Collections.Generic;
using System.IO;
using System.Linq;
using ET.Client;
using Sirenix.Serialization;
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

        public static void OpenBehaviorTree(UnityEngine.Object owner, BTRoot root)
        {
            if (owner == null || root == null)
            {
                return;
            }

            BTNodeDrawer.OpenNode = root;
            BTNodeDrawer.ScriptableObject = owner;
            EditorApplication.ExecuteMenuItem("ET/BehaviorTree/BehaviorTreeEditor");
        }

        public static int GetAssetId(UnityEngine.Object asset)
        {
            return asset switch
            {
                SpellScriptableObject spellAsset => spellAsset.SpellConfig.Id,
                BuffScriptableObject buffAsset => buffAsset.BuffConfig.Id,
                _ => 0,
            };
        }

        public static string GetAssetDirectory(UnityEngine.Object asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            string directory = Path.GetDirectoryName(path);
            return directory?.Replace("\\", "/") ?? SpellEditorConstants.BtAssetRoot;
        }

        public static SpellScriptableObject CreateSpellAsset(int spellId, string directory)
        {
            SpellConfig config = new()
            {
                Id = spellId,
                BuffId = SpellEditorConstants.DefaultBuffId(spellId),
            };
            return CreateSpellAsset(config, directory);
        }

        public static BuffScriptableObject CreateBuffAsset(int buffId, string directory)
        {
            BuffConfig config = new()
            {
                Id = buffId,
            };
            return CreateBuffAsset(config, directory);
        }

        public static SpellScriptableObject CreateSpellAsset(SpellConfig config, string directory)
        {
            if (config == null || config.Id <= 0)
            {
                EditorUtility.DisplayDialog("创建失败", "Spell Id 必须大于 0。", "确定");
                return null;
            }

            EnsureDirectory(directory);
            string path = $"{directory}/{config.Id}.asset";
            if (!EnsurePathAvailable(path))
            {
                return null;
            }

            SpellScriptableObject asset = ScriptableObject.CreateInstance<SpellScriptableObject>();
            asset.SpellConfig = config;
            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.SetDirty(asset);
            SaveAssets();
            return asset;
        }

        public static BuffScriptableObject CreateBuffAsset(BuffConfig config, string directory)
        {
            if (config == null || config.Id <= 0)
            {
                EditorUtility.DisplayDialog("创建失败", "Buff Id 必须大于 0。", "确定");
                return null;
            }

            EnsureDirectory(directory);
            string path = $"{directory}/{config.Id}.asset";
            if (!EnsurePathAvailable(path))
            {
                return null;
            }

            BuffScriptableObject asset = ScriptableObject.CreateInstance<BuffScriptableObject>();
            asset.BuffConfig = config;
            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.SetDirty(asset);
            SaveAssets();
            return asset;
        }

        public static bool DeleteAsset(UnityEngine.Object asset)
        {
            if (asset == null)
            {
                return false;
            }

            string path = AssetDatabase.GetAssetPath(asset);
            bool ok = AssetDatabase.DeleteAsset(path);
            SaveAssets();
            return ok;
        }

        public static bool RenameAssetId(SpellEditorAssetIndex index, UnityEngine.Object asset, int newId)
        {
            if (index == null || asset == null)
            {
                return false;
            }

            if (newId <= 0)
            {
                EditorUtility.DisplayDialog("改 Id 失败", "目标 Id 必须大于 0。", "确定");
                return false;
            }

            if (asset is SpellScriptableObject spellAsset)
            {
                int oldId = spellAsset.SpellConfig.Id;
                if (oldId == newId)
                {
                    return true;
                }

                if (index.Spells.ContainsKey(newId))
                {
                    EditorUtility.DisplayDialog("改 Id 失败", $"Spell Id {newId} 已存在。", "确定");
                    return false;
                }

                spellAsset.SpellConfig.Id = newId;
                RewriteReferences(index, new Dictionary<int, int> { [oldId] = newId }, null);
                return RenameAssetFile(spellAsset, newId);
            }

            if (asset is BuffScriptableObject buffAsset)
            {
                int oldId = buffAsset.BuffConfig.Id;
                if (oldId == newId)
                {
                    return true;
                }

                if (index.Buffs.ContainsKey(newId))
                {
                    EditorUtility.DisplayDialog("改 Id 失败", $"Buff Id {newId} 已存在。", "确定");
                    return false;
                }

                buffAsset.BuffConfig.Id = newId;
                RewriteReferences(index, null, new Dictionary<int, int> { [oldId] = newId });
                return RenameAssetFile(buffAsset, newId);
            }

            return false;
        }

        public static int FindNextSubSpellId(SpellEditorAssetIndex index, int mainSpellId)
        {
            int start = SpellEditorConstants.MainSpellBase(mainSpellId) + 1;
            int end = SpellEditorConstants.MainSpellBase(mainSpellId) + SpellEditorConstants.SpellGroupSize - 1;
            for (int id = start; id <= end; id++)
            {
                if (!index.Spells.ContainsKey(id))
                {
                    return id;
                }
            }

            return 0;
        }

        public static int FindNextMainSpellId(SpellEditorAssetIndex index, int sourceMainSpellId)
        {
            int candidate = SpellEditorConstants.MainSpellBase(sourceMainSpellId) + SpellEditorConstants.SpellGroupSize;
            while (candidate <= int.MaxValue - SpellEditorConstants.SpellGroupSize)
            {
                if (!index.Spells.ContainsKey(candidate))
                {
                    return candidate;
                }

                candidate += SpellEditorConstants.SpellGroupSize;
            }

            return 0;
        }

        public static int FindNextBuffId(SpellEditorAssetIndex index, int baseSpellId)
        {
            int id = SpellEditorConstants.DefaultBuffId(baseSpellId);
            while (index.Buffs.ContainsKey(id))
            {
                id++;
            }

            return id;
        }

        public static bool CopySpellChain(SpellEditorAssetIndex index, SpellEditorBuildResult buildResult, int sourceMainSpellId, int targetMainSpellId)
        {
            if (index == null || buildResult == null)
            {
                return false;
            }

            if (!SpellEditorConstants.IsMainSpell(targetMainSpellId))
            {
                EditorUtility.DisplayDialog("复制失败", "目标主技能 Id 必须是 10 的倍数。", "确定");
                return false;
            }

            if (index.Spells.ContainsKey(targetMainSpellId))
            {
                EditorUtility.DisplayDialog("复制失败", $"Spell Id {targetMainSpellId} 已存在。", "确定");
                return false;
            }

            List<SpellEditorSpellRow> spellRows = buildResult.Spells.Where(x => x.Asset != null).ToList();
            List<SpellEditorBuffRow> buffRows = buildResult.Buffs.Where(x => x.Asset != null).ToList();
            if (spellRows.Count == 0 || !spellRows.Any(x => x.Id == sourceMainSpellId))
            {
                EditorUtility.DisplayDialog("复制失败", "当前链路没有可复制的主技能。", "确定");
                return false;
            }

            if (!TryBuildSpellIdMap(index, spellRows, sourceMainSpellId, targetMainSpellId, out Dictionary<int, int> spellIdMap))
            {
                return false;
            }

            Dictionary<int, int> buffIdMap = BuildBuffIdMap(index, spellRows, buffRows, spellIdMap, targetMainSpellId);
            string targetDirectory = GetCopyDirectory(index, sourceMainSpellId, targetMainSpellId);

            AssetDatabase.StartAssetEditing();
            try
            {
                EnsureDirectory(targetDirectory);
                foreach (SpellEditorSpellRow row in spellRows)
                {
                    SpellConfig config = CloneValue(row.Asset.SpellConfig);
                    config.Id = spellIdMap[row.Id];
                    if (buffIdMap.TryGetValue(config.BuffId, out int mappedBuffId))
                    {
                        config.BuffId = mappedBuffId;
                    }

                    RewriteNodeReferences(config.Cost, spellIdMap, buffIdMap);
                    RewriteNodeReferences(config.TargetSelector, spellIdMap, buffIdMap);
                    CreateSpellAssetWithoutRefresh(config, targetDirectory);
                }

                foreach (SpellEditorBuffRow row in buffRows)
                {
                    BuffConfig config = CloneValue(row.Asset.BuffConfig);
                    config.Id = buffIdMap[row.Id];
                    config.effectDict = null;
                    if (config.Effects != null)
                    {
                        foreach (EffectNode effect in config.Effects)
                        {
                            RewriteNodeReferences(effect, spellIdMap, buffIdMap);
                        }
                    }

                    CreateBuffAssetWithoutRefresh(config, targetDirectory);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            SaveAssets();
            return true;
        }

        private static bool RenameAssetFile(UnityEngine.Object asset, int newId)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            string error = AssetDatabase.RenameAsset(path, newId.ToString());
            if (!string.IsNullOrEmpty(error))
            {
                EditorUtility.DisplayDialog("重命名失败", error, "确定");
                return false;
            }

            EditorUtility.SetDirty(asset);
            SaveAssets();
            return true;
        }

        private static void RewriteReferences(SpellEditorAssetIndex index, Dictionary<int, int> spellIdMap, Dictionary<int, int> buffIdMap)
        {
            foreach (SpellScriptableObject spellAsset in index.Spells.Values)
            {
                if (spellAsset == null)
                {
                    continue;
                }

                bool dirty = false;
                SpellConfig config = spellAsset.SpellConfig;
                if (buffIdMap != null && buffIdMap.TryGetValue(config.BuffId, out int mappedBuffId))
                {
                    config.BuffId = mappedBuffId;
                    dirty = true;
                }

                dirty |= RewriteNodeReferences(config.Cost, spellIdMap, buffIdMap);
                dirty |= RewriteNodeReferences(config.TargetSelector, spellIdMap, buffIdMap);
                if (dirty)
                {
                    EditorUtility.SetDirty(spellAsset);
                }
            }

            foreach (BuffScriptableObject buffAsset in index.Buffs.Values)
            {
                if (buffAsset?.BuffConfig.Effects == null)
                {
                    continue;
                }

                bool dirty = false;
                foreach (EffectNode effect in buffAsset.BuffConfig.Effects)
                {
                    dirty |= RewriteNodeReferences(effect, spellIdMap, buffIdMap);
                }

                if (dirty)
                {
                    EditorUtility.SetDirty(buffAsset);
                }
            }
        }

        private static bool RewriteNodeReferences(BTNode node, Dictionary<int, int> spellIdMap, Dictionary<int, int> buffIdMap)
        {
            if (node == null)
            {
                return false;
            }

            bool dirty = false;
            if (spellIdMap != null && node is BTCreateSpell createSpell && spellIdMap.TryGetValue(createSpell.SpellConfigId, out int mappedSpellId))
            {
                createSpell.SpellConfigId = mappedSpellId;
                dirty = true;
            }

            if (buffIdMap != null && node is BTAddBuff addBuff && buffIdMap.TryGetValue(addBuff.ConfigId, out int mappedBuffId))
            {
                addBuff.ConfigId = mappedBuffId;
                dirty = true;
            }

            if (node.Children == null)
            {
                return dirty;
            }

            foreach (BTNode child in node.Children)
            {
                dirty |= RewriteNodeReferences(child, spellIdMap, buffIdMap);
            }

            return dirty;
        }

        private static bool TryBuildSpellIdMap(
            SpellEditorAssetIndex index,
            List<SpellEditorSpellRow> spellRows,
            int sourceMainSpellId,
            int targetMainSpellId,
            out Dictionary<int, int> spellIdMap)
        {
            spellIdMap = new Dictionary<int, int>();
            HashSet<int> reserved = index.Spells.Keys.ToHashSet();
            int sourceBase = SpellEditorConstants.MainSpellBase(sourceMainSpellId);
            int targetBase = SpellEditorConstants.MainSpellBase(targetMainSpellId);

            foreach (SpellEditorSpellRow row in spellRows.OrderBy(x => x.Id == sourceMainSpellId ? 0 : 1).ThenBy(x => x.Id))
            {
                int targetId;
                if (row.Id == sourceMainSpellId)
                {
                    targetId = targetMainSpellId;
                }
                else if (row.Id >= sourceBase && row.Id < sourceBase + SpellEditorConstants.SpellGroupSize)
                {
                    int offset = row.Id - sourceBase;
                    targetId = targetBase + offset;
                    if (reserved.Contains(targetId))
                    {
                        targetId = FindNextAvailableSubSpellId(reserved, targetBase);
                    }
                }
                else
                {
                    targetId = FindNextAvailableSubSpellId(reserved, targetBase);
                }

                if (targetId == 0)
                {
                    EditorUtility.DisplayDialog("复制失败", $"目标主技能组 {targetBase} 没有足够的子技能 Id。", "确定");
                    return false;
                }

                spellIdMap[row.Id] = targetId;
                reserved.Add(targetId);
            }

            return true;
        }

        private static Dictionary<int, int> BuildBuffIdMap(
            SpellEditorAssetIndex index,
            List<SpellEditorSpellRow> spellRows,
            List<SpellEditorBuffRow> buffRows,
            Dictionary<int, int> spellIdMap,
            int targetMainSpellId)
        {
            Dictionary<int, int> buffIdMap = new();
            HashSet<int> reserved = index.Buffs.Keys.ToHashSet();
            int fallbackBuffId = SpellEditorConstants.DefaultBuffId(targetMainSpellId);

            foreach (SpellEditorBuffRow row in buffRows.OrderBy(x => x.Id))
            {
                SpellEditorSpellRow ownerSpell = spellRows.FirstOrDefault(x => x.Asset.SpellConfig.BuffId == row.Id);
                int targetId = 0;
                if (ownerSpell != null && spellIdMap.TryGetValue(ownerSpell.Id, out int mappedSpellId))
                {
                    targetId = SpellEditorConstants.DefaultBuffId(mappedSpellId);
                }

                if (targetId == 0)
                {
                    targetId = fallbackBuffId;
                }

                while (reserved.Contains(targetId) || buffIdMap.ContainsValue(targetId))
                {
                    targetId++;
                }

                buffIdMap[row.Id] = targetId;
                reserved.Add(targetId);
                fallbackBuffId = targetId + 1;
            }

            return buffIdMap;
        }

        private static int FindNextAvailableSubSpellId(HashSet<int> reserved, int mainSpellBase)
        {
            int start = mainSpellBase + 1;
            int end = mainSpellBase + SpellEditorConstants.SpellGroupSize - 1;
            for (int id = start; id <= end; id++)
            {
                if (!reserved.Contains(id))
                {
                    return id;
                }
            }

            return 0;
        }

        private static string GetCopyDirectory(SpellEditorAssetIndex index, int sourceMainSpellId, int targetMainSpellId)
        {
            string sourceDirectory = index.Spells.TryGetValue(sourceMainSpellId, out SpellScriptableObject sourceAsset)
                    ? GetAssetDirectory(sourceAsset)
                    : SpellEditorConstants.BtAssetRoot;
            string parent = Path.GetDirectoryName(sourceDirectory)?.Replace("\\", "/") ?? SpellEditorConstants.BtAssetRoot;
            string sourceName = Path.GetFileName(sourceDirectory);
            string suffix = sourceName;
            int dashIndex = sourceName.IndexOf('-');
            if (dashIndex >= 0 && dashIndex < sourceName.Length - 1)
            {
                suffix = sourceName[(dashIndex + 1)..];
            }

            string baseDirectory = $"{parent}/{targetMainSpellId}-{suffix}";
            string directory = baseDirectory;
            int duplicateIndex = 2;
            while (Directory.Exists(directory))
            {
                directory = $"{baseDirectory}-{duplicateIndex}";
                duplicateIndex++;
            }

            return directory;
        }

        private static SpellScriptableObject CreateSpellAssetWithoutRefresh(SpellConfig config, string directory)
        {
            SpellScriptableObject asset = ScriptableObject.CreateInstance<SpellScriptableObject>();
            asset.SpellConfig = config;
            AssetDatabase.CreateAsset(asset, $"{directory}/{config.Id}.asset");
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static BuffScriptableObject CreateBuffAssetWithoutRefresh(BuffConfig config, string directory)
        {
            BuffScriptableObject asset = ScriptableObject.CreateInstance<BuffScriptableObject>();
            asset.BuffConfig = config;
            AssetDatabase.CreateAsset(asset, $"{directory}/{config.Id}.asset");
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static T CloneValue<T>(T value)
        {
            byte[] bytes = Sirenix.Serialization.SerializationUtility.SerializeValue(value, DataFormat.Binary);
            return Sirenix.Serialization.SerializationUtility.DeserializeValue<T>(bytes, DataFormat.Binary);
        }

        private static void EnsureDirectory(string directory)
        {
            Directory.CreateDirectory(directory);
        }

        private static bool EnsurePathAvailable(string path)
        {
            if (!File.Exists(path))
            {
                return true;
            }

            EditorUtility.DisplayDialog("创建失败", $"资产已存在：{path}", "确定");
            return false;
        }
    }
}
