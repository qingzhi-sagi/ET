namespace ET.Test
{
    public class Unitybridge_GameViewSetResolutionResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeGameViewResolution resolution = BridgeGameViewResolution.Create();
            resolution.Width = 1920;
            resolution.Height = 1080;
            resolution.Label = "Full HD";
            resolution.IsCurrent = true;

            GameViewSetResolutionResponse value = GameViewSetResolutionResponse.Create();
            value.RpcId = 3702;
            value.Error = 0;
            value.Message = "ok";
            value.Resolution = resolution;
            value.SelectedIndex = 3;
            value.WasAdded = false;
            value.SizeType = "FixedResolution";

            GameViewSetResolutionResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
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
                roundTrip.WasAdded != value.WasAdded ||
                roundTrip.SizeType != value.SizeType)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "GameViewSetResolutionResponse should round-trip set result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
