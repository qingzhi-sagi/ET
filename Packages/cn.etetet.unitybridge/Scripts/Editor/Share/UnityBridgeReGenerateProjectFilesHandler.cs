using System;
using UnityEditor;

namespace ET
{
    internal sealed class UnityBridgeReGenerateProjectFilesHandler : AUnityBridgeDeferredHandler<RegenProject, RegenProjectResponse>
    {
        protected override async ETTask<IResponse> Run(RegenProject command)
        {
            await ETTask.CompletedTask;
            if (EditorApplication.isCompiling)
            {
                throw new Exception("unity is compiling");
            }

            if (!EditorApplication.ExecuteMenuItem("ET/Loader/ReGenerateProjectFiles"))
            {
                throw new Exception("execute menu item failed: ET/Loader/ReGenerateProjectFiles");
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            return null;
        }

        protected override RegenProjectResponse Deferred(RegenProject command, long startedAt)
        {
            if (EditorApplication.isCompiling)
            {
                return null;
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
