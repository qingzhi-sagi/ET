using UnityEditor;

namespace ET
{
    internal sealed class UnityBridgeEditorRedoHandler : AUnityBridgeHandler<EditorRedoRequest, EditorRedoResponse>
    {
        protected override async ETTask<IResponse> Run(EditorRedoRequest command)
        {
            await ETTask.CompletedTask;

            int count = command.Count > 0 ? command.Count : 1;
            for (int i = 0; i < count; ++i)
            {
                Undo.PerformRedo();
            }

            EditorRedoResponse response = EditorRedoResponse.Create();
            response.Count = count;
            return response;
        }
    }
}
