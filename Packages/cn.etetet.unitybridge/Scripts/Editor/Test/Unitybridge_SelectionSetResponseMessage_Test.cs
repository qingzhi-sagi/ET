namespace ET.Test
{
    public class Unitybridge_SelectionSetResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SelectionSetResponse value = SelectionSetResponse.Create();
            value.RpcId = 3202;
            value.Error = 0;
            value.Message = "ok";
            value.SelectedCount = 1;
            value.ActiveObjectName = "Player";
            value.Objects.Add(UnityBridgeProtocolTestSupport.CreateObjectInfo());
            value.Assets.Add(UnityBridgeProtocolTestSupport.CreateAssetInfo());

            SelectionSetResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.SelectedCount != value.SelectedCount ||
                roundTrip.ActiveObjectName != value.ActiveObjectName ||
                roundTrip.Objects.Count != 1 ||
                roundTrip.Assets.Count != 1)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "SelectionSetResponse should round-trip selection result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
