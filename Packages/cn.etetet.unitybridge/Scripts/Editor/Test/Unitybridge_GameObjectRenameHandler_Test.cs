using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_GameObjectRenameHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            string oldName = UnityBridgeHandlerTestSupport.UniqueName("GameObjectRenameHandler");
            string newName = UnityBridgeHandlerTestSupport.UniqueName("GameObjectRenamed");
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(oldName);
            try
            {
                GameObjectRenameRequest request = GameObjectRenameRequest.Create();
                request.Path = oldName;
                request.NewName = newName;

                IResponse rawResponse = await new UnityBridgeGameObjectRenameHandler().Handle(request);
                if (rawResponse is not GameObjectRenameResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "GameObjectRenameHandler should return GameObjectRenameResponse");
                }

                if (response.Error != 0 || response.OldName != oldName || response.NewName != newName || target.name != newName)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "GameObjectRenameHandler should rename the target GameObject");
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
