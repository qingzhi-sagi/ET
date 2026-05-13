namespace ET.Test
{
    public class Unitybridge_BridgeGameViewResolutionMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeGameViewResolution value = BridgeGameViewResolution.Create();
            value.Width = 1920;
            value.Height = 1080;
            value.Label = "Full HD";
            value.IsCurrent = true;

            BridgeGameViewResolution roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.Width != value.Width ||
                roundTrip.Height != value.Height ||
                roundTrip.Label != value.Label ||
                roundTrip.IsCurrent != value.IsCurrent)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgeGameViewResolution should round-trip resolution fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
