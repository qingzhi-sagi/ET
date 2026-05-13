using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_PrefabInstantiateHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject source = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("PrefabInstantiateHandler"));
            string prefabPath = UnityBridgeHandlerTestSupport.SaveTempPrefab(source);
            GameObject instance = null;
            try
            {
                PrefabInstantiateRequest request = PrefabInstantiateRequest.Create();
                request.PrefabPath = prefabPath;
                request.Position = UnityBridgeHandlerTestSupport.CreateVector3(1f, 2f, 3f);

                IResponse rawResponse = await new UnityBridgePrefabInstantiateHandler().Handle(request);
                if (rawResponse is not PrefabInstantiateResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "PrefabInstantiateHandler should return PrefabInstantiateResponse");
                }

                instance = Selection.activeGameObject;
                if (response.Error != 0 || response.Instance == null || instance == null || !PrefabUtility.IsPartOfPrefabInstance(instance))
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "PrefabInstantiateHandler should instantiate prefab into scene");
                }

                if (!UnityBridgeHandlerTestSupport.VectorNearlyEqual(instance.transform.position, 1f, 2f, 3f))
                {
                    return UnityBridgeProtocolTestSupport.Fail(3, "PrefabInstantiateHandler should apply requested position");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                UnityBridgeHandlerTestSupport.DestroyObjects(instance, source);
                UnityBridgeHandlerTestSupport.DeleteTempAsset(prefabPath);
            }
        }
    }
}
