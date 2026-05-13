using UnityEditor;

namespace ET
{
    internal sealed class UnityBridgeEditorGetStateHandler : AUnityBridgeHandler<EditorGetStateRequest, EditorGetStateResponse>
    {
        protected override async ETTask<IResponse> Run(EditorGetStateRequest command)
        {
            await ETTask.CompletedTask;

            EditorGetStateResponse response = EditorGetStateResponse.Create();
            response.IsPlaying = EditorApplication.isPlaying;
            response.IsPaused = EditorApplication.isPaused;
            response.IsCompiling = EditorApplication.isCompiling;
            response.IsUpdating = EditorApplication.isUpdating;
            response.ApplicationPath = EditorApplication.applicationPath;
            response.ApplicationContentsPath = EditorApplication.applicationContentsPath;
            response.EnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            response.EnterPlayModeOptions = EditorSettings.enterPlayModeOptions.ToString();
            return response;
        }
    }
}
