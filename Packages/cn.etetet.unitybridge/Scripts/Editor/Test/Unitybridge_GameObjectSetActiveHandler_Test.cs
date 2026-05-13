using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_GameObjectSetActiveHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("GameObjectSetActiveHandler"));
            try
            {
                GameObjectSetActiveRequest request = GameObjectSetActiveRequest.Create();
                request.Path = UnityBridgeHandlerTestSupport.GetPath(target);
                request.Active = false;

                IResponse rawResponse = await new UnityBridgeGameObjectSetActiveHandler().Handle(request);
                if (rawResponse is not GameObjectSetActiveResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "GameObjectSetActiveHandler should return GameObjectSetActiveResponse");
                }

                if (response.Error != 0 || target.activeSelf || response.ActiveSelf)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "GameObjectSetActiveHandler should update GameObject activeSelf");
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
