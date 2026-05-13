namespace ET.Test
{
    public class Unitybridge_AssetLoadResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeAssetInfo assetInfo = UnityBridgeProtocolTestSupport.CreateAssetInfo();
            AssetLoadResponse value = AssetLoadResponse.Create();
            value.RpcId = 2104;
            value.Error = 0;
            value.Message = "ok";
            value.Asset = assetInfo;
            value.Exists = true;
            value.InstanceId = 3001;

            AssetLoadResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Asset == null ||
                roundTrip.Asset.Guid != assetInfo.Guid ||
                !roundTrip.Exists ||
                roundTrip.InstanceId != value.InstanceId)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "AssetLoadResponse should round-trip asset metadata");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
