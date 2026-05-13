namespace ET.Test
{
    public class Unitybridge_AssetReadTextHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            string fileName = UnityBridgeHandlerTestSupport.UniqueName("AssetReadTextHandler") + ".txt";
            string assetPath = UnityBridgeHandlerTestSupport.WriteTempTextAsset(fileName, "alpha\nbeta\ngamma");
            try
            {
                AssetReadTextRequest request = AssetReadTextRequest.Create();
                request.AssetPath = assetPath;
                request.StartLine = 2;
                request.MaxLines = 1;

                IResponse rawResponse = await new UnityBridgeAssetReadTextHandler().Handle(request);
                if (rawResponse is not AssetReadTextResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "AssetReadTextHandler should return AssetReadTextResponse");
                }

                if (response.Error != 0 ||
                    response.AssetPath != assetPath ||
                    response.TotalLines != 3 ||
                    response.ReturnedLineStart != 2 ||
                    response.ReturnedLineEnd != 2 ||
                    !response.Content.Contains("2: beta"))
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "AssetReadTextHandler should read requested text line range");
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
