using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ET
{
    internal static class UnityBridgeEditorStatus
    {
        public static string GetCodeMode()
        {
            Type globalConfigType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(static assembly => assembly.GetType("ET.GlobalConfig"))
                    .FirstOrDefault(static type => type != null);
            if (globalConfigType == null)
            {
                return string.Empty;
            }

            UnityEngine.Object globalConfig = Resources.Load("GlobalConfig", globalConfigType);
            if (globalConfig == null)
            {
                return string.Empty;
            }

            FieldInfo codeModeField = globalConfigType.GetField("CodeMode");
            return codeModeField?.GetValue(globalConfig)?.ToString() ?? string.Empty;
        }
    }
}
