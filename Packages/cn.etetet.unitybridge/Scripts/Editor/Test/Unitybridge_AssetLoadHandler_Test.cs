namespace ET.Test
{
    public class Unitybridge_AssetLoadHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            string fileName = UnityBridgeHandlerTestSupport.UniqueName("AssetLoadHandler") + ".txt";
            string assetPath = UnityBridgeHandlerTestSupport.WriteTempTextAsset(fileName, "asset load test");
            try
            {
                AssetLoadRequest request = AssetLoadRequest.Create();
                request.AssetPath = assetPath;

                IResponse rawResponse = await new UnityBridgeAssetLoadHandler().Handle(request);
                if (rawResponse is not AssetLoadResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "AssetLoadHandler should return AssetLoadResponse");
                }

                if (response.Error != 0 || !response.Exists || response.Asset == null || response.Asset.AssetPath != assetPath || response.InstanceId == 0)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "AssetLoadHandler should load asset and return asset info");
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
