using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_GameObjectDuplicateHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            string name = UnityBridgeHandlerTestSupport.UniqueName("GameObjectDuplicateHandler");
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(name);
            GameObject duplicate = null;
            try
            {
                GameObjectDuplicateRequest request = GameObjectDuplicateRequest.Create();
                request.Path = name;

                IResponse rawResponse = await new UnityBridgeGameObjectDuplicateHandler().Handle(request);
                if (rawResponse is not GameObjectDuplicateResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "GameObjectDuplicateHandler should return GameObjectDuplicateResponse");
                }

                duplicate = Selection.activeGameObject;
                if (response.Error != 0 || response.Original == null || response.Duplicate == null || duplicate == null)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "GameObjectDuplicateHandler should create duplicate object info");
                }

                if (duplicate == target || duplicate.name != target.name || response.Duplicate.InstanceId != duplicate.GetInstanceID())
                {
                    return UnityBridgeProtocolTestSupport.Fail(3, "GameObjectDuplicateHandler should duplicate the scene object");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                UnityBridgeHandlerTestSupport.DestroyObjects(duplicate, target);
            }
        }
    }
}
