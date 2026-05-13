namespace ET.Test
{
    public class Unitybridge_SelectionGetResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SelectionGetResponse value = SelectionGetResponse.Create();
            value.RpcId = 3201;
            value.Error = 0;
            value.Message = "ok";
            value.Objects.Add(UnityBridgeProtocolTestSupport.CreateObjectInfo());
            value.Assets.Add(UnityBridgeProtocolTestSupport.CreateAssetInfo());
            value.ActiveObjectName = "Player";
            value.ActiveObjectInstanceId = 3001;
            value.Count = 2;

            SelectionGetResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.Objects.Count != 1 ||
                roundTrip.Objects[0].Name != value.Objects[0].Name ||
                roundTrip.Assets.Count != 1 ||
                roundTrip.Assets[0].AssetPath != value.Assets[0].AssetPath ||
                roundTrip.ActiveObjectName != value.ActiveObjectName ||
                roundTrip.ActiveObjectInstanceId != value.ActiveObjectInstanceId ||
                roundTrip.Count != value.Count)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "SelectionGetResponse should round-trip selected objects and assets");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
