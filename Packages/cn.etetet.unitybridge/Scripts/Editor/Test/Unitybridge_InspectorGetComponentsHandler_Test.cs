using System.Linq;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_InspectorGetComponentsHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("InspectorGetComponentsHandler"));
            target.AddComponent<BoxCollider>();
            try
            {
                InspectorGetComponentsRequest request = InspectorGetComponentsRequest.Create();
                request.Path = target.name;

                IResponse rawResponse = await new UnityBridgeInspectorGetComponentsHandler().Handle(request);
                if (rawResponse is not InspectorGetComponentsResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "InspectorGetComponentsHandler should return InspectorGetComponentsResponse");
                }

                if (response.Error != 0 || response.GameObjectName != target.name || response.Count != response.Components.Count)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "InspectorGetComponentsHandler should return component list for scene object");
                }

                if (!response.Components.Any(component => component.TypeName == nameof(Transform)) ||
                    !response.Components.Any(component => component.TypeName == nameof(BoxCollider)))
                {
                    return UnityBridgeProtocolTestSupport.Fail(3, "InspectorGetComponentsHandler should include Transform and added component");
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
