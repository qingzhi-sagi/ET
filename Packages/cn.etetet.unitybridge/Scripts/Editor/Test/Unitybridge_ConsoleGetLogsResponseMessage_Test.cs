namespace ET.Test
{
    public class Unitybridge_ConsoleGetLogsResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeConsoleLog log = BridgeConsoleLog.Create();
            log.LogType = "Error";
            log.Message = "boom";
            log.StackTrace = "stack";
            log.Time = "2026-05-14T10:00:00Z";

            ConsoleGetLogsResponse value = ConsoleGetLogsResponse.Create();
            value.RpcId = 3402;
            value.Error = 0;
            value.Message = "ok";
            value.Logs.Add(log);
            value.Count = 1;
            value.TotalCount = 7;
            value.LogType = "Error";

            ConsoleGetLogsResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.Logs.Count != 1 ||
                roundTrip.Logs[0].Message != log.Message ||
                roundTrip.Count != value.Count ||
                roundTrip.TotalCount != value.TotalCount ||
                roundTrip.LogType != value.LogType)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "ConsoleGetLogsResponse should round-trip logs and summary fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
