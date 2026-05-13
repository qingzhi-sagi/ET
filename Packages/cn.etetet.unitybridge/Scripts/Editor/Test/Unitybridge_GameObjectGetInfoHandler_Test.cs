using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_GameObjectGetInfoHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject parent = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("GameObjectGetInfoHandler"));
            GameObject childA = UnityBridgeHandlerTestSupport.CreateSceneObject("ChildA", parent.transform);
            GameObject childB = UnityBridgeHandlerTestSupport.CreateSceneObject("ChildB", parent.transform);
            try
            {
                GameObjectGetInfoRequest request = GameObjectGetInfoRequest.Create();
                request.Path = parent.name;
                request.IncludeComponents = true;
                request.MaxChildren = 1;

                IResponse rawResponse = await new UnityBridgeGameObjectGetInfoHandler().Handle(request);
                if (rawResponse is not GameObjectGetInfoResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "GameObjectGetInfoHandler should return GameObjectGetInfoResponse");
                }

                if (response.Error != 0 || response.Object == null || response.ChildCount != 2 || response.Children.Count != 1)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "GameObjectGetInfoHandler should return object info and capped child names");
                }

                if (response.Object.Name != parent.name || response.Children[0] != childA.name || response.Object.Components.Count == 0)
                {
                    return UnityBridgeProtocolTestSupport.Fail(3, "GameObjectGetInfoHandler should include requested component and child data");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                UnityBridgeHandlerTestSupport.DestroyObjects(childB, childA, parent);
            }
        }
    }
}
