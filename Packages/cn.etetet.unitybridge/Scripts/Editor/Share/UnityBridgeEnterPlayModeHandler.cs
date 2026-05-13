using System;
using UnityEditor;

namespace ET
{
    internal sealed class UnityBridgeEnterPlayModeHandler : AUnityBridgeDeferredHandler<EnterPlay, EnterPlayResponse>
    {
        protected override async ETTask<EnterPlayResponse> Run(EnterPlay command, UnityBridgeDeferredContext deferred)
        {
            if (!deferred.IsResuming && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new Exception("unity already in playmode or changing playmode");
            }

            await deferred.Defer(() => EditorApplication.isPlaying = true);

            if (EditorApplication.isPlaying)
            {
                EnterPlayResponse response = EnterPlayResponse.Create();
                response.Message = "unity entered playmode";
                response.IsPlaying = true;
                return response;
            }

            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return deferred.NotReady<EnterPlayResponse>();
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
