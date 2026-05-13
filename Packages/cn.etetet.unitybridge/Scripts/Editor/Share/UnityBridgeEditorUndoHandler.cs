using UnityEditor;

namespace ET
{
    internal sealed class UnityBridgeEditorUndoHandler : AUnityBridgeHandler<EditorUndoRequest, EditorUndoResponse>
    {
        protected override async ETTask<IResponse> Run(EditorUndoRequest command)
        {
            await ETTask.CompletedTask;

            int count = command.Count > 0 ? command.Count : 1;
            for (int i = 0; i < count; ++i)
            {
                Undo.PerformUndo();
            }

            EditorUndoResponse response = EditorUndoResponse.Create();
            response.Count = count;
            return response;
        }
    }
}
