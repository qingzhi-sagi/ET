using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_EditorRedoHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("EditorRedoHandler"));
            string originalName = target.name;
            string changedName = originalName + "_Changed";
            try
            {
                Undo.IncrementCurrentGroup();
                Undo.RecordObject(target, "UnityBridge editor redo test");
                target.name = changedName;
                Undo.FlushUndoRecordObjects();
                Undo.PerformUndo();

                EditorRedoRequest request = EditorRedoRequest.Create();
                request.Count = 1;

                IResponse rawResponse = await new UnityBridgeEditorRedoHandler().Handle(request);
                if (rawResponse is not EditorRedoResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "EditorRedoHandler should return EditorRedoResponse");
                }

                if (response.Error != 0 || response.Count != 1 || target.name != changedName)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "EditorRedoHandler should perform Unity redo operation");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                UnityBridgeHandlerTestSupport.DestroyObjects(target);
            }
        }
    }
}
