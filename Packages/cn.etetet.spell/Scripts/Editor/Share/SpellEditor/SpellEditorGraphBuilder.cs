using System.Collections.Generic;
using ET.Client;
using UnityEditor;

namespace ET
{
    public static class SpellEditorGraphBuilder
    {
        public static SpellEditorBuildResult Build(SpellEditorAssetIndex index, int mainSpellId)
        {
            SpellEditorBuildResult result = new();
            HashSet<int> visitedSpells = new();
            HashSet<int> visitedBuffs = new();

            AddSpell(index, result, mainSpellId, new SpellEditorSourceInfo { Kind = SpellEditorReferenceKind.MainSpell, OwnerId = mainSpellId }, visitedSpells, visitedBuffs);
            return result;
        }

        private static void AddSpell(
            SpellEditorAssetIndex index,
            SpellEditorBuildResult result,
            int spellId,
            SpellEditorSourceInfo source,
            HashSet<int> visitedSpells,
            HashSet<int> visitedBuffs)
        {
            SpellEditorSpellRow row = FindOrCreateSpellRow(index, result, spellId);
            row.Sources.Add(source);

            if (!visitedSpells.Add(spellId))
            {
                return;
            }

            SpellScriptableObject asset = row.Asset;
            if (asset == null)
            {
                result.Issues.Add(new SpellEditorIssue(MessageType.Error, $"缺失 SpellConfig: {spellId}"));
                return;
            }

            SpellConfig config = asset.SpellConfig;
            string assetPath = index.GetPath(asset);
            AddBuff(index, result, config.BuffId, new SpellEditorSourceInfo
            {
                Kind = SpellEditorReferenceKind.SpellBuffId,
                OwnerId = config.Id,
                OwnerPath = assetPath,
                NodePath = "BuffId",
            }, visitedSpells, visitedBuffs);

            List<SpellEditorReference> references = new();
            SpellEditorReferenceScanner.ScanSpell(config, assetPath, references);
            ExpandReferences(index, result, references, visitedSpells, visitedBuffs);
        }

        private static void AddBuff(
            SpellEditorAssetIndex index,
            SpellEditorBuildResult result,
            int buffId,
            SpellEditorSourceInfo source,
            HashSet<int> visitedSpells,
            HashSet<int> visitedBuffs)
        {
            SpellEditorBuffRow row = FindOrCreateBuffRow(index, result, buffId);
            row.Sources.Add(source);

            if (!visitedBuffs.Add(buffId))
            {
                return;
            }

            BuffScriptableObject asset = row.Asset;
            if (asset == null)
            {
                result.Issues.Add(new SpellEditorIssue(MessageType.Error, $"缺失 BuffConfig: {buffId}"));
                return;
            }

            List<SpellEditorReference> references = new();
            SpellEditorReferenceScanner.ScanBuff(asset.BuffConfig, index.GetPath(asset), references);
            ExpandReferences(index, result, references, visitedSpells, visitedBuffs);
        }

        private static void ExpandReferences(
            SpellEditorAssetIndex index,
            SpellEditorBuildResult result,
            List<SpellEditorReference> references,
            HashSet<int> visitedSpells,
            HashSet<int> visitedBuffs)
        {
            foreach (SpellEditorReference reference in references)
            {
                SpellEditorSourceInfo source = new()
                {
                    Kind = reference.Kind,
                    OwnerId = reference.OwnerId,
                    OwnerPath = reference.OwnerPath,
                    NodePath = reference.NodePath,
                };

                if (reference.Kind == SpellEditorReferenceKind.CreateSpell)
                {
                    AddSpell(index, result, reference.TargetId, source, visitedSpells, visitedBuffs);
                    continue;
                }

                if (reference.Kind == SpellEditorReferenceKind.AddBuff)
                {
                    AddBuff(index, result, reference.TargetId, source, visitedSpells, visitedBuffs);
                }
            }
        }

        private static SpellEditorSpellRow FindOrCreateSpellRow(SpellEditorAssetIndex index, SpellEditorBuildResult result, int spellId)
        {
            foreach (SpellEditorSpellRow row in result.Spells)
            {
                if (row.Id == spellId)
                {
                    return row;
                }
            }

            index.Spells.TryGetValue(spellId, out SpellScriptableObject asset);
            SpellEditorSpellRow created = new()
            {
                Kind = asset == null? SpellEditorRowKind.Missing : SpellEditorRowKind.Normal,
                Id = spellId,
                Asset = asset,
                AssetPath = index.GetPath(asset),
            };
            result.Spells.Add(created);
            return created;
        }

        private static SpellEditorBuffRow FindOrCreateBuffRow(SpellEditorAssetIndex index, SpellEditorBuildResult result, int buffId)
        {
            foreach (SpellEditorBuffRow row in result.Buffs)
            {
                if (row.Id == buffId)
                {
                    return row;
                }
            }

            index.Buffs.TryGetValue(buffId, out BuffScriptableObject asset);
            SpellEditorBuffRow created = new()
            {
                Kind = asset == null? SpellEditorRowKind.Missing : SpellEditorRowKind.Normal,
                Id = buffId,
                Asset = asset,
                AssetPath = index.GetPath(asset),
            };
            result.Buffs.Add(created);
            return created;
        }
    }
}
