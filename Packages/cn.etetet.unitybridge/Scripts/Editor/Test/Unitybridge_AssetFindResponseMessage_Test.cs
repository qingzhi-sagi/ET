namespace ET.Test
{
    public class Unitybridge_AssetFindResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeAssetInfo assetInfo = UnityBridgeProtocolTestSupport.CreateAssetInfo();
            AssetFindResponse value = AssetFindResponse.Create();
            value.RpcId = 2102;
            value.Error = 0;
            value.Message = "ok";
            value.Assets.Add(assetInfo);
            value.Paths.Add(assetInfo.AssetPath);
            value.Filter = "t:Prefab";
            value.TotalFound = 4;
            value.Returned = 1;

            AssetFindResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Assets.Count != 1 ||
                roundTrip.Assets[0].Guid != assetInfo.Guid ||
                roundTrip.Paths.Count != 1 ||
                roundTrip.Filter != value.Filter ||
                roundTrip.TotalFound != value.TotalFound ||
                roundTrip.Returned != value.Returned)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "AssetFindResponse should round-trip find response fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
