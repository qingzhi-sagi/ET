namespace ET.Test
{
    public class Unitybridge_AssetFindHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            string fileName = UnityBridgeHandlerTestSupport.UniqueName("AssetFindHandler") + ".txt";
            string assetPath = UnityBridgeHandlerTestSupport.WriteTempTextAsset(fileName, "asset find test");
            try
            {
                AssetFindRequest request = AssetFindRequest.Create();
                request.Filter = System.IO.Path.GetFileNameWithoutExtension(fileName);
                request.Format = "paths";
                request.MaxResults = 10;
                request.SearchInFolders.Add("Assets/__UnityBridgeHandlerTests");

                IResponse rawResponse = await new UnityBridgeAssetFindHandler().Handle(request);
                if (rawResponse is not AssetFindResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "AssetFindHandler should return AssetFindResponse");
                }

                if (response.Error != 0 || response.Returned != 1 || response.Paths.Count != 1 || response.Paths[0] != assetPath)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "AssetFindHandler should return matching asset path");
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
