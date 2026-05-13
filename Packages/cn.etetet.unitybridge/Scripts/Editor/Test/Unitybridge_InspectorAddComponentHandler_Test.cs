using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_InspectorAddComponentHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("InspectorAddComponentHandler"));
            try
            {
                InspectorAddComponentRequest request = InspectorAddComponentRequest.Create();
                request.Path = target.name;
                request.TypeName = nameof(BoxCollider);

                IResponse rawResponse = await new UnityBridgeInspectorAddComponentHandler().Handle(request);
                if (rawResponse is not InspectorAddComponentResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "InspectorAddComponentHandler should return InspectorAddComponentResponse");
                }

                if (response.Error != 0 || response.AddedComponent == null || target.GetComponent<BoxCollider>() == null)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "InspectorAddComponentHandler should add requested component");
                }

                if (response.AddedComponent.TypeName != nameof(BoxCollider))
                {
                    return UnityBridgeProtocolTestSupport.Fail(3, "InspectorAddComponentHandler should report added component info");
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
