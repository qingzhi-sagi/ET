namespace ET.Test
{
    public class Unitybridge_AssetReadTextResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            AssetReadTextResponse value = AssetReadTextResponse.Create();
            value.RpcId = 2107;
            value.Error = 0;
            value.Message = "ok";
            value.AssetPath = "Assets/Scripts/Player.cs";
            value.TotalLines = 20;
            value.ReturnedLineStart = 2;
            value.ReturnedLineEnd = 4;
            value.ReturnedLineCount = 3;
            value.Truncated = true;
            value.Content = "2: line";

            AssetReadTextResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.AssetPath != value.AssetPath ||
                roundTrip.TotalLines != value.TotalLines ||
                roundTrip.ReturnedLineStart != value.ReturnedLineStart ||
                roundTrip.ReturnedLineEnd != value.ReturnedLineEnd ||
                roundTrip.ReturnedLineCount != value.ReturnedLineCount ||
                roundTrip.Truncated != value.Truncated ||
                roundTrip.Content != value.Content)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "AssetReadTextResponse should round-trip typed text fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
