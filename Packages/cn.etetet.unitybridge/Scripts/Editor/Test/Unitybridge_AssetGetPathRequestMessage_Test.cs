namespace ET.Test
{
    public class Unitybridge_AssetGetPathRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            AssetGetPathRequest value = AssetGetPathRequest.Create();
            value.RpcId = 2103;
            value.Guid = "guid-player";

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(AssetGetPathRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            AssetGetPathRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Guid != value.Guid)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "AssetGetPathRequest should round-trip guid");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
