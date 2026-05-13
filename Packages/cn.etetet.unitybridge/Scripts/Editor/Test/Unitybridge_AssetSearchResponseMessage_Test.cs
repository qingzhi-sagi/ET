namespace ET.Test
{
    public class Unitybridge_AssetSearchResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeAssetInfo assetInfo = UnityBridgeProtocolTestSupport.CreateAssetInfo();
            AssetSearchResponse value = AssetSearchResponse.Create();
            value.RpcId = 2101;
            value.Error = 0;
            value.Message = "ok";
            value.Assets.Add(assetInfo);
            value.Paths.Add(assetInfo.AssetPath);
            value.Mode = "prefab";
            value.Filter = "t:Prefab Player";
            value.TotalFound = 2;
            value.Returned = 1;

            AssetSearchResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.Assets.Count != 1 ||
                roundTrip.Assets[0].AssetPath != assetInfo.AssetPath ||
                roundTrip.Paths.Count != 1 ||
                roundTrip.Paths[0] != assetInfo.AssetPath ||
                roundTrip.Mode != value.Mode ||
                roundTrip.Filter != value.Filter ||
                roundTrip.TotalFound != value.TotalFound ||
                roundTrip.Returned != value.Returned)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "AssetSearchResponse should round-trip typed assets and result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
