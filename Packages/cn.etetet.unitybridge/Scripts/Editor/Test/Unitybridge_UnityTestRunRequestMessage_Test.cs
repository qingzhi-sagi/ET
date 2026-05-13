namespace ET.Test
{
    public class Unitybridge_UnityTestRunRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            UnityTestRunRequest value = UnityTestRunRequest.Create();
            value.RpcId = 3301;
            value.Name = "Unitybridge_.*RequestMessage_Test";

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(UnityTestRunRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            UnityTestRunRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Name != value.Name)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "UnityTestRunRequest should round-trip name regex");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
