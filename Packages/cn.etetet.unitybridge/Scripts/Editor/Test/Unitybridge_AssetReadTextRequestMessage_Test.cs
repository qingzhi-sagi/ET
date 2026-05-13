namespace ET.Test
{
    public class Unitybridge_AssetReadTextRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            AssetReadTextRequest value = AssetReadTextRequest.Create();
            value.RpcId = 2107;
            value.AssetPath = "Assets/Scripts/Player.cs";
            value.StartLine = 2;
            value.MaxLines = 20;
            value.MaxChars = 1000;

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(AssetReadTextRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            AssetReadTextRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.AssetPath != value.AssetPath ||
                roundTrip.StartLine != value.StartLine ||
                roundTrip.MaxLines != value.MaxLines ||
                roundTrip.MaxChars != value.MaxChars)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "AssetReadTextRequest should round-trip bounded text fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
