namespace ET.Test
{
    public class Unitybridge_EditorLogRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            EditorLogRequest value = EditorLogRequest.Create();
            value.RpcId = 3403;
            value.Message = "hello";
            value.LogType = "Log";

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(EditorLogRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            EditorLogRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Message != value.Message ||
                roundTrip.LogType != value.LogType)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "EditorLogRequest should round-trip log fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
