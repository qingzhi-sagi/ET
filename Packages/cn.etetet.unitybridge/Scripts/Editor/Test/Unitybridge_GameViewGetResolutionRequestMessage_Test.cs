namespace ET.Test
{
    public class Unitybridge_GameViewGetResolutionRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            GameViewGetResolutionRequest value = GameViewGetResolutionRequest.Create();
            value.RpcId = 3601;

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(GameViewGetResolutionRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            GameViewGetResolutionRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null || roundTrip.RpcId != value.RpcId)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "GameViewGetResolutionRequest should round-trip rpc id");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
