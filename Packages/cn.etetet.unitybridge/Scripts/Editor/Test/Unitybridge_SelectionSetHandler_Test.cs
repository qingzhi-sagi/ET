using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_SelectionSetHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("SelectionSetHandler"));
            try
            {
                SelectionSetRequest request = SelectionSetRequest.Create();
                request.Path = target.name;

                IResponse rawResponse = await new UnityBridgeSelectionSetHandler().Handle(request);
                if (rawResponse is not SelectionSetResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "SelectionSetHandler should return SelectionSetResponse");
                }

                if (response.Error != 0 || response.SelectedCount != 1 || Selection.activeObject != target || response.ActiveObjectName != target.name)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "SelectionSetHandler should set Unity selection");
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
