using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_InspectorRemoveComponentHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("InspectorRemoveComponentHandler"));
            target.AddComponent<BoxCollider>();
            try
            {
                InspectorRemoveComponentRequest request = InspectorRemoveComponentRequest.Create();
                request.Path = target.name;
                request.ComponentName = nameof(BoxCollider);
                request.ComponentIndex = -1;

                IResponse rawResponse = await new UnityBridgeInspectorRemoveComponentHandler().Handle(request);
                if (rawResponse is not InspectorRemoveComponentResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "InspectorRemoveComponentHandler should return InspectorRemoveComponentResponse");
                }

                if (response.Error != 0 || response.RemovedComponent == null || target.GetComponent<BoxCollider>() != null)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "InspectorRemoveComponentHandler should remove selected component");
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
