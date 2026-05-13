namespace ET.Test
{
    public class Unitybridge_AssetFindRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            AssetFindRequest value = AssetFindRequest.Create();
            value.RpcId = 2102;
            value.Filter = "t:Material";
            value.SearchInFolders.Add("Assets/Materials");
            value.MaxResults = 10;
            value.Format = "paths";

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(AssetFindRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            AssetFindRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Filter != value.Filter ||
                roundTrip.SearchInFolders.Count != 1 ||
                roundTrip.SearchInFolders[0] != value.SearchInFolders[0] ||
                roundTrip.MaxResults != value.MaxResults ||
                roundTrip.Format != value.Format)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "AssetFindRequest should round-trip find request fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
