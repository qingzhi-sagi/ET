namespace ET.Test
{
    public class Unitybridge_ScreenshotCaptureResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeScreenshotInfo info = BridgeScreenshotInfo.Create();
            info.Path = "/tmp/unitybridge/screenshot.png";
            info.FileName = "screenshot.png";
            info.Width = 1280;
            info.Height = 720;
            info.FileSize = 4096;
            info.MediaType = "image/png";

            ScreenshotCaptureResponse value = ScreenshotCaptureResponse.Create();
            value.RpcId = 3502;
            value.Error = 0;
            value.Message = "ok";
            value.Captured = true;
            value.Target = "game";
            value.Screenshot = info;

            ScreenshotCaptureResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.Captured != value.Captured ||
                roundTrip.Target != value.Target ||
                roundTrip.Screenshot == null ||
                roundTrip.Screenshot.Path != info.Path ||
                roundTrip.Screenshot.MediaType != info.MediaType)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "ScreenshotCaptureResponse should round-trip capture result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
