namespace ET.Test
{
    public class Unitybridge_GameViewGetResolutionHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameViewGetResolutionRequest request = GameViewGetResolutionRequest.Create();

            IResponse rawResponse = await new UnityBridgeGameViewGetResolutionHandler().Handle(request);
            if (rawResponse is not GameViewGetResolutionResponse response)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "GameViewGetResolutionHandler should return GameViewGetResolutionResponse");
            }

            if (response.Error != 0)
            {
                return string.IsNullOrWhiteSpace(response.Message)
                        ? UnityBridgeProtocolTestSupport.Fail(2, "GameViewGetResolutionHandler failure should include message")
                        : ErrorCode.ERR_Success;
            }

            if (response.Resolution == null ||
                response.Resolution.Width <= 0 ||
                response.Resolution.Height <= 0 ||
                !response.Resolution.IsCurrent)
            {
                return UnityBridgeProtocolTestSupport.Fail(3, "GameViewGetResolutionHandler should return a current positive resolution on success");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
