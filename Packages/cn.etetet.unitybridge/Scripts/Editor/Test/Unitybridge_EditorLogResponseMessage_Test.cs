namespace ET.Test
{
    public class Unitybridge_EditorLogResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            EditorLogResponse value = EditorLogResponse.Create();
            value.RpcId = 3404;
            value.Error = 0;
            value.Message = "ok";
            value.Logged = true;
            value.LogType = "Warning";
            value.LoggedMessage = "[UnityBridge] hello";

            EditorLogResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.Logged != value.Logged ||
                roundTrip.LogType != value.LogType ||
                roundTrip.LoggedMessage != value.LoggedMessage)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "EditorLogResponse should round-trip log result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
