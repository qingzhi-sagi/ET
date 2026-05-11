using System;
using UnityEditor;

namespace ET
{
    internal sealed class UnityBridgeExitPlayModeHandler : AUnityBridgeDeferredHandler<ExitPlay, ExitPlayResponse>
    {
        protected override async ETTask<IResponse> Run(ExitPlay command)
        {
            await ETTask.CompletedTask;
            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new Exception("unity not in playmode");
            }

            EditorApplication.isPlaying = false;
            return null;
        }

        protected override ExitPlayResponse Deferred(ExitPlay command, long startedAt)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return null;
            }

            ExitPlayResponse response = ExitPlayResponse.Create();
            if (!EditorApplication.isPlaying)
            {
                response.Message = "unity exited playmode";
                response.IsPlaying = false;
                return response;
            }

            response.Error = UnityBridgeErrorCode.HandlerFail;
            response.Message = "unity failed to exit playmode";
            response.IsPlaying = true;
            return response;
        }
    }
}
