namespace ET.Test
{
    public class Unitybridge_SelectionGetRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SelectionGetRequest value = SelectionGetRequest.Create();
            value.RpcId = 3201;
            value.IncludeComponents = true;

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(SelectionGetRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            SelectionGetRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.IncludeComponents != value.IncludeComponents)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "SelectionGetRequest should round-trip include components flag");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
