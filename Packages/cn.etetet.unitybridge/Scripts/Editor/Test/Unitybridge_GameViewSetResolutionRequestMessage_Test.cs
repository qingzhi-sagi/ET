namespace ET.Test
{
    public class Unitybridge_GameViewSetResolutionRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            GameViewSetResolutionRequest value = GameViewSetResolutionRequest.Create();
            value.RpcId = 3701;
            value.Width = 1920;
            value.Height = 1080;
            value.Label = "Full HD";

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(GameViewSetResolutionRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            GameViewSetResolutionRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Width != value.Width ||
                roundTrip.Height != value.Height ||
                roundTrip.Label != value.Label)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "GameViewSetResolutionRequest should round-trip resolution fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
