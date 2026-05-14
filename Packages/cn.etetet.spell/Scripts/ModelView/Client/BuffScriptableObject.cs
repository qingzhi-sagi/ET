#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ET.Client
{
    [CreateAssetMenu(menuName = "ET/BuffScriptableObject")]
    [EnableClass]
    [HideMonoScript]
    public class BuffScriptableObject : SerializedScriptableObject
    {
        private const string BuffAssetPrefix = "b";
        private const string LegacyBuffAssetPrefix = "Buff";

        [Title("Buff 配置", TitleAlignment = TitleAlignments.Centered)]
        [NonSerialized, OdinSerialize]
        [HideLabel]
        [HideReferenceObjectPicker]
        public BuffConfig BuffConfig = new();

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!TryParseAssetName(BuffAssetPrefix, LegacyBuffAssetPrefix, this.name, out int id))
            {
                return;
            }

            this.BuffConfig.Id = id;    // ScriptableObject 的 name 就是 asset 名称
#endif
        }

        private static bool TryParseAssetName(string prefix, string legacyPrefix, string assetName, out int id)
        {
            id = 0;
            if (string.IsNullOrEmpty(assetName))
            {
                return false;
            }

            string idText = assetName;
            if (assetName.StartsWith(prefix, StringComparison.Ordinal))
            {
                idText = assetName.Substring(prefix.Length);
            }
            else if (assetName.StartsWith(legacyPrefix, StringComparison.Ordinal))
            {
                idText = assetName.Substring(legacyPrefix.Length);
            }

            return TryParseId(idText, out id);
        }

        private static bool TryParseId(string idText, out int id)
        {
            id = 0;
            if (string.IsNullOrEmpty(idText))
            {
                return false;
            }

            foreach (char c in idText)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            return int.TryParse(idText, out id);
        }
    }
}

#endif
