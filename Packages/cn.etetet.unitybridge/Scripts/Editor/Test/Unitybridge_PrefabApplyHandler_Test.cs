using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_PrefabApplyHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject source = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("PrefabApplyHandler"));
            string prefabPath = UnityBridgeHandlerTestSupport.SaveTempPrefab(source);
            UnityBridgeHandlerTestSupport.DestroyObjects(source);
            source = null;
            GameObject instance = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)) as GameObject;
            try
            {
                instance.transform.localPosition = new Vector3(9f, 0f, 0f);
                PrefabApplyRequest request = PrefabApplyRequest.Create();
                request.GameObjectPath = instance.name;

                IResponse rawResponse = await new UnityBridgePrefabApplyHandler().Handle(request);
                if (rawResponse is not PrefabApplyResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "PrefabApplyHandler should return PrefabApplyResponse");
                }

                if (response.Error != 0 || !response.Applied || response.PrefabPath != prefabPath)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "PrefabApplyHandler should apply prefab instance changes");
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
