namespace ET.Test
{
    public class Unitybridge_AssetGetPathResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeAssetInfo assetInfo = UnityBridgeProtocolTestSupport.CreateAssetInfo();
            AssetGetPathResponse value = AssetGetPathResponse.Create();
            value.RpcId = 2103;
            value.Error = 0;
            value.Message = "ok";
            value.Guid = assetInfo.Guid;
            value.AssetPath = assetInfo.AssetPath;
            value.Exists = true;
            value.Asset = assetInfo;

            AssetGetPathResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Guid != value.Guid ||
                roundTrip.AssetPath != value.AssetPath ||
                !roundTrip.Exists ||
                roundTrip.Asset == null ||
                roundTrip.Asset.AssetPath != assetInfo.AssetPath)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "AssetGetPathResponse should round-trip path result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
