namespace ET.Test
{
    public class Unitybridge_BridgeConsoleLogMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeConsoleLog value = BridgeConsoleLog.Create();
            value.LogType = "Error";
            value.Message = "boom";
            value.StackTrace = "stack";
            value.Time = "2026-05-13T23:00:00Z";

            BridgeConsoleLog roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.LogType != value.LogType ||
                roundTrip.Message != value.Message ||
                roundTrip.StackTrace != value.StackTrace ||
                roundTrip.Time != value.Time)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgeConsoleLog should round-trip log fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
