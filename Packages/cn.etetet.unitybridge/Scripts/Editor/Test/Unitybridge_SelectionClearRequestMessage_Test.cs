namespace ET.Test
{
    public class Unitybridge_SelectionClearRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SelectionClearRequest value = SelectionClearRequest.Create();
            value.RpcId = 3205;

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(SelectionClearRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            SelectionClearRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null || roundTrip.RpcId != value.RpcId)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "SelectionClearRequest should round-trip rpc id");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
