using System;
using UnityEditor;

namespace ET
{
    internal sealed class UnityBridgeCompileScriptsHandler : AUnityBridgeDeferredHandler<Compile, CompileResponse>
    {
        protected override async ETTask<IResponse> Run(Compile command)
        {
            await ETTask.CompletedTask;
            if (EditorApplication.isCompiling)
            {
                throw new Exception("unity is compiling");
            }

            if (!EditorApplication.ExecuteMenuItem("ET/Scripts/Compile"))
            {
                throw new Exception("execute menu item failed: ET/Scripts/Compile");
            }

            return null;
        }

        protected override CompileResponse Deferred(Compile command, long startedAt)
        {
            if (EditorApplication.isCompiling)
            {
                return null;
            }

            CompileResponse response = CompileResponse.Create();
            if (EditorUtility.scriptCompilationFailed)
            {
                response.Error = UnityBridgeErrorCode.HandlerFail;
                response.Message = "unity compile finished with compile errors";
                return response;
            }

            response.Message = "unity compile completed";
            response.DurationMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startedAt;
            return response;
        }
    }
}
