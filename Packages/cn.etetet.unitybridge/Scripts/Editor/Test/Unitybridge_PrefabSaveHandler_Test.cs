using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_PrefabSaveHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject source = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("PrefabSaveHandler"));
            string prefabPath = UnityBridgeHandlerTestSupport.CreateTempAssetPath(source.name + ".prefab");
            try
            {
                PrefabSaveRequest request = PrefabSaveRequest.Create();
                request.GameObjectPath = source.name;
                request.SavePath = prefabPath;

                IResponse rawResponse = await new UnityBridgePrefabSaveHandler().Handle(request);
                if (rawResponse is not PrefabSaveResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "PrefabSaveHandler should return PrefabSaveResponse");
                }

                if (response.Error != 0 || !response.Saved || response.Asset == null || AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "PrefabSaveHandler should save scene object as prefab asset");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                UnityBridgeHandlerTestSupport.DestroyObjects(source);
                UnityBridgeHandlerTestSupport.DeleteTempAsset(prefabPath);
            }
        }
    }
}
