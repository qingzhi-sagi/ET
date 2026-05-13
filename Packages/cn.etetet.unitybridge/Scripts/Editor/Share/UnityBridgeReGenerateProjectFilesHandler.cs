using System;
using UnityEditor;

namespace ET
{
    internal sealed class UnityBridgeReGenerateProjectFilesHandler : AUnityBridgeDeferredHandler<RegenProject, RegenProjectResponse>
    {
        protected override async ETTask<RegenProjectResponse> Run(RegenProject command, UnityBridgeDeferredContext deferred)
        {
            if (!deferred.IsResuming && EditorApplication.isCompiling)
            {
                throw new Exception("unity is compiling");
            }

            await deferred.Defer(() =>
            {
                if (!EditorApplication.ExecuteMenuItem("ET/Loader/ReGenerateProjectFiles"))
                {
                    throw new Exception("execute menu item failed: ET/Loader/ReGenerateProjectFiles");
                }

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            });

            if (EditorApplication.isCompiling)
            {
                return deferred.NotReady<RegenProjectResponse>();
            }

            RegenProjectResponse response = RegenProjectResponse.Create();
            if (EditorUtility.scriptCompilationFailed)
            {
                response.Error = UnityBridgeErrorCode.HandlerFail;
                response.Message = "unity regenerate project files finished with compile errors";
                return response;
            }

            response.Message = "unity regenerate project files completed";
            return response;
        }
    }
}
