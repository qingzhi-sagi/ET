namespace ET.Test
{
    public class Unitybridge_AssetRefreshRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            AssetRefreshRequest value = AssetRefreshRequest.Create();
            value.RpcId = 2106;
            value.ForceUpdate = true;

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(AssetRefreshRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            AssetRefreshRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.ForceUpdate != value.ForceUpdate)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "AssetRefreshRequest should round-trip force update");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
