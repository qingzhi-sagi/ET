namespace ET.Test
{
    public class Unitybridge_BridgeScreenshotInfoMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeScreenshotInfo value = BridgeScreenshotInfo.Create();
            value.Path = "Temp/UnityBridge/screenshots/game.png";
            value.FileName = "game.png";
            value.Width = 1280;
            value.Height = 720;
            value.FileSize = 2048;
            value.MediaType = "image/png";

            BridgeScreenshotInfo roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.Path != value.Path ||
                roundTrip.FileName != value.FileName ||
                roundTrip.Width != value.Width ||
                roundTrip.Height != value.Height ||
                roundTrip.FileSize != value.FileSize ||
                roundTrip.MediaType != value.MediaType)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgeScreenshotInfo should round-trip screenshot fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
