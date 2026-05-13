namespace ET.Test
{
    public class Unitybridge_ConsoleGetLogsRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            ConsoleGetLogsRequest value = ConsoleGetLogsRequest.Create();
            value.RpcId = 3401;
            value.Count = 25;
            value.LogType = "Warning";

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(ConsoleGetLogsRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            ConsoleGetLogsRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Count != value.Count ||
                roundTrip.LogType != value.LogType)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "ConsoleGetLogsRequest should round-trip log query fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
