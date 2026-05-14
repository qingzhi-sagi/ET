using System;
using UnityEditor;
using UnityEngine;

namespace ET
{
    internal sealed class UnityBridgePingHandler : AUnityBridgeHandler<Ping, PingResponse>
    {
        protected override async ETTask<IResponse> Run(Ping command)
        {
            await ETTask.CompletedTask;
            PingResponse response = PingResponse.Create();
            response.Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            response.IsCompiling = EditorApplication.isCompiling;
            response.IsPlaying = EditorApplication.isPlaying;
            response.IsPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode;
            response.CodeMode = UnityBridgeEditorStatus.GetCodeMode();
            response.UnityVersion = Application.unityVersion;
            return response;
        }
    }
}
