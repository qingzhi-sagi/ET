using UnityEditor;

namespace ET
{
    internal sealed class UnityBridgeEditorPauseHandler : AUnityBridgeHandler<EditorPauseRequest, EditorPauseResponse>
    {
        protected override async ETTask<IResponse> Run(EditorPauseRequest command)
        {
            await ETTask.CompletedTask;

            if (command.Toggle)
            {
                EditorApplication.isPaused = !EditorApplication.isPaused;
            }
            else
            {
                EditorApplication.isPaused = command.Pause;
            }

            EditorPauseResponse response = EditorPauseResponse.Create();
            response.IsPaused = EditorApplication.isPaused;
            return response;
        }
    }
}
