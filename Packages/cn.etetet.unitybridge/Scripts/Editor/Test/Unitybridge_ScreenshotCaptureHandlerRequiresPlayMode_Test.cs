using UnityEditor;

namespace ET.Test
{
    public class Unitybridge_ScreenshotCaptureHandlerRequiresPlayMode_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;
            if (EditorApplication.isPlaying)
            {
                return ErrorCode.ERR_Success;
            }

            ScreenshotCaptureRequest request = ScreenshotCaptureRequest.Create();
            request.Target = "game";
            request.Format = "png";
            request.AllowEditMode = false;

            IResponse rawResponse = await new UnityBridgeScreenshotCaptureHandler().Handle(request);
            if (rawResponse is not ScreenshotCaptureResponse response)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "ScreenshotCaptureHandler should return ScreenshotCaptureResponse");
            }

            if (response.Error != UnityBridgeErrorCode.NotInPlayMode ||
                response.Captured ||
                response.Target != "game" ||
                string.IsNullOrWhiteSpace(response.Message))
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "ScreenshotCaptureHandler should reject Game view capture outside PlayMode by default");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
