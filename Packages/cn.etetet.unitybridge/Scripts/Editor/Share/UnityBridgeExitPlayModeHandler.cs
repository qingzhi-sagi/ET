using System;
using UnityEditor;

namespace ET
{
    internal sealed class UnityBridgeExitPlayModeHandler : AUnityBridgeDeferredHandler<ExitPlay, ExitPlayResponse>
    {
        protected override async ETTask<ExitPlayResponse> Run(ExitPlay command, UnityBridgeDeferredContext deferred)
        {
            await ETTask.CompletedTask;
            
            if (!deferred.IsResuming && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new Exception("unity not in playmode");
            }

            if (!deferred.IsResuming)
            {
                EditorApplication.isPlaying = false;
                return deferred.Started<ExitPlayResponse>();
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return deferred.NotReady<ExitPlayResponse>();
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
