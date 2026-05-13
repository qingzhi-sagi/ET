namespace ET.Test
{
    public class Unitybridge_SelectionClearResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SelectionClearResponse value = SelectionClearResponse.Create();
            value.RpcId = 3205;
            value.Error = 0;
            value.Message = "ok";
            value.Cleared = true;
            value.SelectedCount = 0;

            SelectionClearResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.Cleared != value.Cleared ||
                roundTrip.SelectedCount != value.SelectedCount)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "SelectionClearResponse should round-trip clear result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
