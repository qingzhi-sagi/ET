using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_SelectionClearHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("SelectionClearHandler"));
            try
            {
                Selection.objects = new UnityEngine.Object[] { target };
                SelectionClearRequest request = SelectionClearRequest.Create();

                IResponse rawResponse = await new UnityBridgeSelectionClearHandler().Handle(request);
                if (rawResponse is not SelectionClearResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "SelectionClearHandler should return SelectionClearResponse");
                }

                if (response.Error != 0 || !response.Cleared || response.SelectedCount != 0 || Selection.objects.Length != 0)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "SelectionClearHandler should clear Unity selection");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                Selection.objects = System.Array.Empty<UnityEngine.Object>();
                UnityBridgeHandlerTestSupport.DestroyObjects(target);
            }
        }
    }
}
