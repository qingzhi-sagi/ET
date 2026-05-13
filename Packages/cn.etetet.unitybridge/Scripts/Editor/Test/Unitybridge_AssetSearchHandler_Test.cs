namespace ET.Test
{
    public class Unitybridge_AssetSearchHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            string fileName = UnityBridgeHandlerTestSupport.UniqueName("AssetSearchHandler") + ".txt";
            string assetPath = UnityBridgeHandlerTestSupport.WriteTempTextAsset(fileName, "asset search test");
            try
            {
                AssetSearchRequest request = AssetSearchRequest.Create();
                request.Mode = "all";
                request.Keyword = System.IO.Path.GetFileNameWithoutExtension(fileName);
                request.Format = "full";
                request.MaxResults = 10;
                request.SearchInFolders.Add("Assets/__UnityBridgeHandlerTests");

                IResponse rawResponse = await new UnityBridgeAssetSearchHandler().Handle(request);
                if (rawResponse is not AssetSearchResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "AssetSearchHandler should return AssetSearchResponse");
                }

                if (response.Error != 0 || response.Returned != 1 || response.Paths.Count != 1 || response.Paths[0] != assetPath)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "AssetSearchHandler should find matching asset in requested folder");
                }

                if (response.Assets.Count != 1 || response.Assets[0].AssetPath != assetPath)
                {
                    return UnityBridgeProtocolTestSupport.Fail(3, "AssetSearchHandler should return full asset info when requested");
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
