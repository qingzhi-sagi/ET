using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_PrefabUnpackHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject source = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("PrefabUnpackHandler"));
            string prefabPath = UnityBridgeHandlerTestSupport.SaveTempPrefab(source);
            UnityBridgeHandlerTestSupport.DestroyObjects(source);
            source = null;
            GameObject instance = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)) as GameObject;
            try
            {
                PrefabUnpackRequest request = PrefabUnpackRequest.Create();
                request.GameObjectPath = instance.name;
                request.Completely = true;

                IResponse rawResponse = await new UnityBridgePrefabUnpackHandler().Handle(request);
                if (rawResponse is not PrefabUnpackResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "PrefabUnpackHandler should return PrefabUnpackResponse");
                }

                if (response.Error != 0 || !response.Unpacked || PrefabUtility.IsPartOfAnyPrefab(instance))
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "PrefabUnpackHandler should unpack prefab instance");
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
