using UnityEditor;

namespace ET
{
    internal sealed class UnityBridgeReloadHotfixHandler : AUnityBridgeHandler<Reload, ReloadResponse>
    {
        protected override async ETTask<IResponse> Run(Reload command)
        {
            await ETTask.CompletedTask;
            if (!EditorApplication.isPlaying)
            {
                ReloadResponse response = ReloadResponse.Create();
                response.Error = UnityBridgeErrorCode.NotInPlayMode;
                response.Message = "reload hotfix requires playmode";
                return response;
            }

            bool success = EditorApplication.ExecuteMenuItem("ET/Scripts/Reload");
            if (!success)
            {
                ReloadResponse response = ReloadResponse.Create();
                response.Error = UnityBridgeErrorCode.HandlerFail;
                response.Message = "execute menu item failed: ET/Scripts/Reload";
                return response;
            }

            return ReloadResponse.Create();
        }
    }
}
