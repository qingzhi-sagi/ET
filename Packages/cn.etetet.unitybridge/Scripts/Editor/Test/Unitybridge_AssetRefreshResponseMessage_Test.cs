namespace ET.Test
{
    public class Unitybridge_AssetRefreshResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            AssetRefreshResponse value = AssetRefreshResponse.Create();
            value.RpcId = 2106;
            value.Error = 0;
            value.Message = "asset refresh completed";
            value.Refreshed = true;

            AssetRefreshResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.Refreshed != value.Refreshed)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "AssetRefreshResponse should round-trip refresh result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
