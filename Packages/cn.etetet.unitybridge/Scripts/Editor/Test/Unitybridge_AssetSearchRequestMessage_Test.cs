namespace ET.Test
{
    public class Unitybridge_AssetSearchRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            AssetSearchRequest value = AssetSearchRequest.Create();
            value.RpcId = 2101;
            value.Mode = "prefab";
            value.Filter = "t:Prefab";
            value.Keyword = "Player";
            value.SearchInFolders.Add("Assets/Prefabs");
            value.MaxResults = 5;
            value.Format = "full";

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(AssetSearchRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            AssetSearchRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Mode != value.Mode ||
                roundTrip.Filter != value.Filter ||
                roundTrip.Keyword != value.Keyword ||
                roundTrip.SearchInFolders.Count != 1 ||
                roundTrip.SearchInFolders[0] != value.SearchInFolders[0] ||
                roundTrip.MaxResults != value.MaxResults ||
                roundTrip.Format != value.Format)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "AssetSearchRequest should round-trip search request fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
