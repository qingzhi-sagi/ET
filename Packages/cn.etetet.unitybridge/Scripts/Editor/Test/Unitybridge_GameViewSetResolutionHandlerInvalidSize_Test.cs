namespace ET.Test
{
    public class Unitybridge_GameViewSetResolutionHandlerInvalidSize_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameViewSetResolutionRequest request = GameViewSetResolutionRequest.Create();
            request.Width = 0;
            request.Height = 720;

            IResponse rawResponse = await new UnityBridgeGameViewSetResolutionHandler().Handle(request);
            if (rawResponse is not GameViewSetResolutionResponse response)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "GameViewSetResolutionHandler should return GameViewSetResolutionResponse");
            }

            if (response.Error != UnityBridgeErrorCode.InvalidCommandLine ||
                string.IsNullOrWhiteSpace(response.Message) ||
                response.Resolution != null ||
                response.SelectedIndex != -1 ||
                response.WasAdded)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "GameViewSetResolutionHandler should reject invalid resolution dimensions");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
