using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_EditorUndoHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("EditorUndoHandler"));
            string originalName = target.name;
            string changedName = originalName + "_Changed";
            try
            {
                Undo.IncrementCurrentGroup();
                Undo.RecordObject(target, "UnityBridge editor undo test");
                target.name = changedName;
                Undo.FlushUndoRecordObjects();

                EditorUndoRequest request = EditorUndoRequest.Create();
                request.Count = 1;

                IResponse rawResponse = await new UnityBridgeEditorUndoHandler().Handle(request);
                if (rawResponse is not EditorUndoResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "EditorUndoHandler should return EditorUndoResponse");
                }

                if (response.Error != 0 || response.Count != 1 || target.name != originalName)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "EditorUndoHandler should perform Unity undo operation");
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
