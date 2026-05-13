namespace ET.Test
{
    public class Unitybridge_GameViewGetResolutionResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeGameViewResolution resolution = BridgeGameViewResolution.Create();
            resolution.Width = 1920;
            resolution.Height = 1080;
            resolution.Label = "Full HD";
            resolution.IsCurrent = true;

            GameViewGetResolutionResponse value = GameViewGetResolutionResponse.Create();
            value.RpcId = 3602;
            value.Error = 0;
            value.Message = "ok";
            value.Resolution = resolution;
            value.SelectedIndex = 3;
            value.SizeType = "FixedResolution";

            GameViewGetResolutionResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.Resolution == null ||
                roundTrip.Resolution.Width != resolution.Width ||
                roundTrip.Resolution.Height != resolution.Height ||
                roundTrip.Resolution.Label != resolution.Label ||
                !roundTrip.Resolution.IsCurrent ||
                roundTrip.SelectedIndex != value.SelectedIndex ||
                roundTrip.SizeType != value.SizeType)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "GameViewGetResolutionResponse should round-trip current resolution fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
