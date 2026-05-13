namespace ET.Test
{
    public class Unitybridge_GameViewListResolutionsResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeGameViewResolution current = BridgeGameViewResolution.Create();
            current.Width = 1280;
            current.Height = 720;
            current.Label = "HD";
            current.IsCurrent = true;

            BridgeGameViewResolution other = BridgeGameViewResolution.Create();
            other.Width = 1920;
            other.Height = 1080;
            other.Label = "Full HD";

            GameViewListResolutionsResponse value = GameViewListResolutionsResponse.Create();
            value.RpcId = 3604;
            value.Error = 0;
            value.Message = "ok";
            value.Resolutions.Add(current);
            value.Resolutions.Add(other);
            value.Count = 2;
            value.CurrentIndex = 0;

            GameViewListResolutionsResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.Resolutions.Count != 2 ||
                roundTrip.Resolutions[0].Width != current.Width ||
                roundTrip.Resolutions[0].IsCurrent != current.IsCurrent ||
                roundTrip.Resolutions[1].Label != other.Label ||
                roundTrip.Count != value.Count ||
                roundTrip.CurrentIndex != value.CurrentIndex)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "GameViewListResolutionsResponse should round-trip resolution list fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
