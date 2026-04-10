using System;
using UnityEditor;

namespace ET
{
    internal sealed class UnityBridgeRefreshHandler : AUnityBridgeDeferredHandler<Refresh, RefreshResponse>
    {
        protected override async ETTask<IResponse> Run(Refresh command)
        {
            await ETTask.CompletedTask;
            if (EditorApplication.isCompiling)
            {
                throw new Exception("unity is compiling");
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            return null;
        }

        protected override RefreshResponse Deferred(Refresh command, long startedAt)
        {
            if (EditorApplication.isCompiling)
            {
                return null;
            }

            RefreshResponse response = RefreshResponse.Create();
            if (EditorUtility.scriptCompilationFailed)
            {
                response.Error = UnityBridgeErrorCode.HandlerFail;
                response.Message = "unity refresh finished with compile errors";
                return response;
            }

            response.Message = "unity refresh completed";
            return response;
        }
    }
}
