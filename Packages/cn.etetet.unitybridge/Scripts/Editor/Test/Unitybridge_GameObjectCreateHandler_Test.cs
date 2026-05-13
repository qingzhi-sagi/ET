using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_GameObjectCreateHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            string name = UnityBridgeHandlerTestSupport.UniqueName("GameObjectCreateHandler");
            GameObject created = null;
            try
            {
                GameObjectCreateRequest request = GameObjectCreateRequest.Create();
                request.Name = name;
                request.PrimitiveType = "Cube";

                IResponse rawResponse = await new UnityBridgeGameObjectCreateHandler().Handle(request);
                if (rawResponse is not GameObjectCreateResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "GameObjectCreateHandler should return GameObjectCreateResponse");
                }

                created = Selection.activeGameObject;
                if (response.Error != 0 || !response.Created || response.Object == null || created == null)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "GameObjectCreateHandler should create and select a GameObject");
                }

                if (created.name != name || created.GetComponent<BoxCollider>() == null || response.Object.InstanceId != created.GetInstanceID())
                {
                    return UnityBridgeProtocolTestSupport.Fail(3, "GameObjectCreateHandler should create requested primitive and return object info");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                UnityBridgeHandlerTestSupport.DestroyObjects(created);
            }
        }
    }
}
