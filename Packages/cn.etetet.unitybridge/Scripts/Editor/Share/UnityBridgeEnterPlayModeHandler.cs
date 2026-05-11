using System;
using UnityEditor;

namespace ET
{
    internal sealed class UnityBridgeEnterPlayModeHandler : AUnityBridgeDeferredHandler<EnterPlay, EnterPlayResponse>
    {
        protected override async ETTask<IResponse> Run(EnterPlay command)
        {
            await ETTask.CompletedTask;
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new Exception("unity already in playmode or changing playmode");
            }

            EditorApplication.isPlaying = true;
            return null;
        }

        protected override EnterPlayResponse Deferred(EnterPlay command, long startedAt)
        {
            if (EditorApplication.isPlaying)
            {
                EnterPlayResponse response = EnterPlayResponse.Create();
                response.Message = "unity entered playmode";
                response.IsPlaying = true;
                return response;
            }

            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return null;
            }

            EnterPlayResponse failure = EnterPlayResponse.Create();
            failure.IsPlaying = false;
            failure.Error = UnityBridgeErrorCode.HandlerFail;
            failure.Message = EditorUtility.scriptCompilationFailed
                    ? "enter playmode failed because scripts failed to compile"
                    : "unity failed to enter playmode";
            return failure;
        }
    }
}
