namespace ET.Test
{
    public class Unitybridge_ScreenshotCaptureRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            ScreenshotCaptureRequest value = ScreenshotCaptureRequest.Create();
            value.RpcId = 3501;
            value.Target = "game";
            value.Format = "png";
            value.Quality = 90;
            value.AllowEditMode = true;

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(ScreenshotCaptureRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            ScreenshotCaptureRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Target != value.Target ||
                roundTrip.Format != value.Format ||
                roundTrip.Quality != value.Quality ||
                roundTrip.AllowEditMode != value.AllowEditMode)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "ScreenshotCaptureRequest should round-trip capture options");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
