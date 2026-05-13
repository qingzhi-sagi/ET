namespace ET.Test
{
    public class Unitybridge_SelectionRemoveResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SelectionRemoveResponse value = SelectionRemoveResponse.Create();
            value.RpcId = 3204;
            value.Error = 0;
            value.Message = "ok";
            value.Removed = true;
            value.ObjectName = "Enemy";
            value.SelectedCount = 1;
            value.Objects.Add(UnityBridgeProtocolTestSupport.CreateObjectInfo());

            SelectionRemoveResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.Removed != value.Removed ||
                roundTrip.ObjectName != value.ObjectName ||
                roundTrip.SelectedCount != value.SelectedCount ||
                roundTrip.Objects.Count != 1)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "SelectionRemoveResponse should round-trip remove result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
