using UnityEditor;

namespace ET.Test
{
    public class Unitybridge_AssetGetPathHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            string fileName = UnityBridgeHandlerTestSupport.UniqueName("AssetGetPathHandler") + ".txt";
            string assetPath = UnityBridgeHandlerTestSupport.WriteTempTextAsset(fileName, "asset get path test");
            try
            {
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                AssetGetPathRequest request = AssetGetPathRequest.Create();
                request.Guid = guid;

                IResponse rawResponse = await new UnityBridgeAssetGetPathHandler().Handle(request);
                if (rawResponse is not AssetGetPathResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "AssetGetPathHandler should return AssetGetPathResponse");
                }

                if (response.Error != 0 || !response.Exists || response.Guid != guid || response.AssetPath != assetPath || response.Asset == null)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "AssetGetPathHandler should resolve guid to asset path");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                UnityBridgeHandlerTestSupport.DeleteTempAsset(assetPath);
            }
        }
    }
}
