using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_PrefabGetInfoHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject source = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("PrefabGetInfoHandler"));
            string prefabPath = UnityBridgeHandlerTestSupport.SaveTempPrefab(source);
            try
            {
                PrefabGetInfoRequest request = PrefabGetInfoRequest.Create();
                request.PrefabPath = prefabPath;

                IResponse rawResponse = await new UnityBridgePrefabGetInfoHandler().Handle(request);
                if (rawResponse is not PrefabGetInfoResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "PrefabGetInfoHandler should return PrefabGetInfoResponse");
                }

                if (response.Error != 0 || response.Name != source.name || !response.IsPrefabAsset)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "PrefabGetInfoHandler should report prefab asset info");
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
