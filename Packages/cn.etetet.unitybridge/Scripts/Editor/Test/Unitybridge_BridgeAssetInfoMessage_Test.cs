namespace ET.Test
{
    public class Unitybridge_BridgeAssetInfoMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeAssetInfo value = UnityBridgeProtocolTestSupport.CreateAssetInfo();
            BridgeAssetInfo roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.AssetPath != value.AssetPath ||
                roundTrip.Guid != value.Guid ||
                roundTrip.TypeName != value.TypeName ||
                roundTrip.Name != value.Name ||
                roundTrip.Extension != value.Extension ||
                roundTrip.FileSize != value.FileSize ||
                roundTrip.InstanceId != value.InstanceId)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgeAssetInfo should round-trip asset metadata fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
