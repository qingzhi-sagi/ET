using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_SelectionGetHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("SelectionGetHandler"));
            try
            {
                Selection.activeObject = target;
                SelectionGetRequest request = SelectionGetRequest.Create();
                request.IncludeComponents = true;

                IResponse rawResponse = await new UnityBridgeSelectionGetHandler().Handle(request);
                if (rawResponse is not SelectionGetResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "SelectionGetHandler should return SelectionGetResponse");
                }

                if (response.Error != 0 || response.Count != 1 || response.ActiveObjectName != target.name || response.Objects.Count != 1)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "SelectionGetHandler should read Unity selection");
                }

                if (response.Objects[0].Components.Count == 0)
                {
                    return UnityBridgeProtocolTestSupport.Fail(3, "SelectionGetHandler should include components when requested");
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
