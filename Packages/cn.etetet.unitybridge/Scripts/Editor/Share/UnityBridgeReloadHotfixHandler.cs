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
                ReloadResponse notPlayingResponse = ReloadResponse.Create();
                notPlayingResponse.Error = UnityBridgeErrorCode.NotInPlayMode;
                notPlayingResponse.Message = "unity not in playmode";
                return notPlayingResponse;
            }

            if (EditorApplication.ExecuteMenuItem("ET/Scripts/Reload"))
            {
                return ReloadResponse.Create();
            }

            ReloadResponse response = ReloadResponse.Create();
            response.Error = UnityBridgeErrorCode.HandlerFail;
            response.Message = "execute menu item failed: ET/Scripts/Reload";
            return response;
        }
    }
}
