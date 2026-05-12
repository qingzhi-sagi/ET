using System.Collections.Generic;
using ET.Client;
using UnityEditor;

namespace ET
{
    public enum SpellEditorRowKind
    {
        Normal,
        Missing,
    }

    public enum SpellEditorReferenceKind
    {
        MainSpell,
        SpellBuffId,
        CreateSpell,
        AddBuff,
    }

    public sealed class SpellEditorSourceInfo
    {
        public SpellEditorReferenceKind Kind;
        public int OwnerId;
        public string OwnerPath;
        public string NodePath;

        public override string ToString()
        {
            return string.IsNullOrEmpty(this.NodePath)
                    ? $"{this.Kind} {this.OwnerId}"
                    : $"{this.Kind} {this.OwnerId} @ {this.NodePath}";
        }
    }

    public sealed class SpellEditorSpellRow
    {
        public SpellEditorRowKind Kind;
        public int Id;
        public SpellScriptableObject Asset;
        public string AssetPath;
        public readonly List<SpellEditorSourceInfo> Sources = new();

        public SpellConfig Config => this.Asset != null? this.Asset.SpellConfig : null;
        public bool IsMain => SpellEditorConstants.IsMainSpell(this.Id);
    }

    public sealed class SpellEditorBuffRow
    {
        public SpellEditorRowKind Kind;
        public int Id;
        public BuffScriptableObject Asset;
        public string AssetPath;
        public readonly List<SpellEditorSourceInfo> Sources = new();

        public BuffConfig Config => this.Asset != null? this.Asset.BuffConfig : null;
    }

    public sealed class SpellEditorIssue
    {
        public MessageType Severity;
        public string Message;

        public SpellEditorIssue(MessageType severity, string message)
        {
            this.Severity = severity;
            this.Message = message;
        }
    }

    public sealed class SpellEditorBuildResult
    {
        public readonly List<SpellEditorSpellRow> Spells = new();
        public readonly List<SpellEditorBuffRow> Buffs = new();
        public readonly List<SpellEditorIssue> Issues = new();
    }
}
