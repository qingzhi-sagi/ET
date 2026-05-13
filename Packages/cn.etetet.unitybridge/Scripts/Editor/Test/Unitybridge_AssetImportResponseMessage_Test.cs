namespace ET.Test
{
    public class Unitybridge_AssetImportResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeAssetInfo assetInfo = UnityBridgeProtocolTestSupport.CreateAssetInfo();
            AssetImportResponse value = AssetImportResponse.Create();
            value.RpcId = 2105;
            value.Error = 0;
            value.Message = "imported";
            value.AssetPath = assetInfo.AssetPath;
            value.Imported = true;
            value.Asset = assetInfo;

            AssetImportResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Message != value.Message ||
                roundTrip.AssetPath != value.AssetPath ||
                !roundTrip.Imported ||
                roundTrip.Asset == null ||
                roundTrip.Asset.TypeName != assetInfo.TypeName)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "AssetImportResponse should round-trip import result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
