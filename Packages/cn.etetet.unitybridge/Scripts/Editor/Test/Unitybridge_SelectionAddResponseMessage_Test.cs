namespace ET.Test
{
    public class Unitybridge_SelectionAddResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SelectionAddResponse value = SelectionAddResponse.Create();
            value.RpcId = 3203;
            value.Error = 0;
            value.Message = "ok";
            value.Added = true;
            value.ObjectName = "Enemy";
            value.SelectedCount = 2;
            value.Objects.Add(UnityBridgeProtocolTestSupport.CreateObjectInfo("Enemy", "Root/Enemy", 3002));

            SelectionAddResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.Added != value.Added ||
                roundTrip.ObjectName != value.ObjectName ||
                roundTrip.SelectedCount != value.SelectedCount ||
                roundTrip.Objects.Count != 1)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "SelectionAddResponse should round-trip add result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
