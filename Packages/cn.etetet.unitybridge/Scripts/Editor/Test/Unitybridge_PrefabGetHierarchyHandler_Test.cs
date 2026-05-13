using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_PrefabGetHierarchyHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject source = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("PrefabGetHierarchyHandler"));
            GameObject child = UnityBridgeHandlerTestSupport.CreateSceneObject("Child", source.transform);
            string prefabPath = UnityBridgeHandlerTestSupport.SaveTempPrefab(source);
            try
            {
                PrefabGetHierarchyRequest request = PrefabGetHierarchyRequest.Create();
                request.PrefabPath = prefabPath;
                request.Depth = 2;
                request.IncludeComponents = true;

                IResponse rawResponse = await new UnityBridgePrefabGetHierarchyHandler().Handle(request);
                if (rawResponse is not PrefabGetHierarchyResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "PrefabGetHierarchyHandler should return PrefabGetHierarchyResponse");
                }

                if (response.Error != 0 || response.RootCount != 1 || response.Roots.Count != 1 || response.Roots[0].Children.Count != 1)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "PrefabGetHierarchyHandler should return prefab hierarchy");
                }

                if (response.Roots[0].Object.Name != source.name || response.Roots[0].Children[0].Object.Name != child.name)
                {
                    return UnityBridgeProtocolTestSupport.Fail(3, "PrefabGetHierarchyHandler should preserve prefab node names");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                UnityBridgeHandlerTestSupport.DestroyObjects(child, source);
                UnityBridgeHandlerTestSupport.DeleteTempAsset(prefabPath);
            }
        }
    }
}
