using System.Linq;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_GameObjectFindHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            string name = UnityBridgeHandlerTestSupport.UniqueName("GameObjectFindHandler");
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(name);
            target.AddComponent<BoxCollider>();
            try
            {
                GameObjectFindRequest request = GameObjectFindRequest.Create();
                request.Name = name;
                request.WithComponent = nameof(BoxCollider);
                request.IncludeComponents = true;
                request.MaxResults = 1;

                IResponse rawResponse = await new UnityBridgeGameObjectFindHandler().Handle(request);
                if (rawResponse is not GameObjectFindResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "GameObjectFindHandler should return GameObjectFindResponse");
                }

                if (response.Error != 0 || response.Count != 1 || response.Objects.Count != 1)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "GameObjectFindHandler should find the matching scene object");
                }

                BridgeObjectInfo info = response.Objects[0];
                if (info.Name != name || !info.Components.Any(component => component.TypeName == nameof(BoxCollider)))
                {
                    return UnityBridgeProtocolTestSupport.Fail(3, "GameObjectFindHandler should return component details when requested");
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
