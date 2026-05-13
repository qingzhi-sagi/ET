using System.Linq;

namespace ET.Test
{
    public class Unitybridge_GameViewListResolutionsHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameViewListResolutionsRequest request = GameViewListResolutionsRequest.Create();

            IResponse rawResponse = await new UnityBridgeGameViewListResolutionsHandler().Handle(request);
            if (rawResponse is not GameViewListResolutionsResponse response)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "GameViewListResolutionsHandler should return GameViewListResolutionsResponse");
            }

            if (response.Error != 0)
            {
                return string.IsNullOrWhiteSpace(response.Message)
                        ? UnityBridgeProtocolTestSupport.Fail(2, "GameViewListResolutionsHandler failure should include message")
                        : ErrorCode.ERR_Success;
            }

            if (response.Count != response.Resolutions.Count ||
                response.Count <= 0 ||
                !response.Resolutions.Any(value => value.IsCurrent))
            {
                return UnityBridgeProtocolTestSupport.Fail(3, "GameViewListResolutionsHandler should return counted resolutions with current marker on success");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
