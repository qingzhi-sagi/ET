using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_SelectionAddHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject first = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("SelectionAddFirst"));
            GameObject second = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("SelectionAddSecond"));
            try
            {
                Selection.objects = new UnityEngine.Object[] { first };
                SelectionAddRequest request = SelectionAddRequest.Create();
                request.Path = second.name;

                IResponse rawResponse = await new UnityBridgeSelectionAddHandler().Handle(request);
                if (rawResponse is not SelectionAddResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "SelectionAddHandler should return SelectionAddResponse");
                }

                if (response.Error != 0 || !response.Added || response.SelectedCount != 2 || Selection.objects.Length != 2)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "SelectionAddHandler should add target to current selection");
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
