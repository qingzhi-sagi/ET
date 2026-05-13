using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_GameObjectDestroyHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("GameObjectDestroyHandler"));
            string path = UnityBridgeHandlerTestSupport.GetPath(target);

            GameObjectDestroyRequest request = GameObjectDestroyRequest.Create();
            request.Path = path;

            IResponse rawResponse = await new UnityBridgeGameObjectDestroyHandler().Handle(request);
            if (rawResponse is not GameObjectDestroyResponse response)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "GameObjectDestroyHandler should return GameObjectDestroyResponse");
            }

            if (response.Error != 0 || !response.Destroyed || response.DestroyedPath != path || target != null)
            {
                UnityBridgeHandlerTestSupport.DestroyObjects(target);
                return UnityBridgeProtocolTestSupport.Fail(2, "GameObjectDestroyHandler should destroy the target GameObject");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
