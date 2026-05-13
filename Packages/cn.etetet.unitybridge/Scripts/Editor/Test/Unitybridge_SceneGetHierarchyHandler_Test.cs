using System.Linq;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_SceneGetHierarchyHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject root = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("SceneGetHierarchyHandler"));
            GameObject child = UnityBridgeHandlerTestSupport.CreateSceneObject("Child", root.transform);
            try
            {
                SceneGetHierarchyRequest request = SceneGetHierarchyRequest.Create();
                request.Depth = 2;
                request.IncludeInactive = true;

                IResponse rawResponse = await new UnityBridgeSceneGetHierarchyHandler().Handle(request);
                if (rawResponse is not SceneGetHierarchyResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "SceneGetHierarchyHandler should return SceneGetHierarchyResponse");
                }

                BridgeSceneNode node = response.Roots.FirstOrDefault(value => value.Object != null && value.Object.Name == root.name);
                if (response.Error != 0 || node == null || node.Children.Count != 1 || node.Children[0].Object.Name != child.name)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "SceneGetHierarchyHandler should include active scene hierarchy");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                UnityBridgeHandlerTestSupport.DestroyObjects(child, root);
            }
        }
    }
}
