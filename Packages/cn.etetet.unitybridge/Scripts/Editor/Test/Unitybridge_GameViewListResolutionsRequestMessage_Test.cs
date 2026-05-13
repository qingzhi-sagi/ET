namespace ET.Test
{
    public class Unitybridge_GameViewListResolutionsRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            GameViewListResolutionsRequest value = GameViewListResolutionsRequest.Create();
            value.RpcId = 3603;

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(GameViewListResolutionsRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            GameViewListResolutionsRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null || roundTrip.RpcId != value.RpcId)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "GameViewListResolutionsRequest should round-trip rpc id");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
