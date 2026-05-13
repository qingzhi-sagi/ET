using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_SelectionRemoveHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject first = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("SelectionRemoveFirst"));
            GameObject second = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("SelectionRemoveSecond"));
            try
            {
                Selection.objects = new UnityEngine.Object[] { first, second };
                SelectionRemoveRequest request = SelectionRemoveRequest.Create();
                request.Path = first.name;

                IResponse rawResponse = await new UnityBridgeSelectionRemoveHandler().Handle(request);
                if (rawResponse is not SelectionRemoveResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "SelectionRemoveHandler should return SelectionRemoveResponse");
                }

                if (response.Error != 0 || !response.Removed || response.SelectedCount != 1 || Selection.objects.Length != 1 || Selection.objects[0] != second)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "SelectionRemoveHandler should remove target from current selection");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                Selection.objects = System.Array.Empty<UnityEngine.Object>();
                UnityBridgeHandlerTestSupport.DestroyObjects(second, first);
            }
        }
    }
}
