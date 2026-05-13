namespace ET.Test
{
    public class Unitybridge_EditorLogHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            EditorLogRequest request = EditorLogRequest.Create();
            request.Message = "Unitybridge_EditorLogHandler_Test";
            request.LogType = "Warning";

            IResponse rawResponse = await new UnityBridgeEditorLogHandler().Handle(request);
            if (rawResponse is not EditorLogResponse response)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "EditorLogHandler should return EditorLogResponse");
            }

            if (response.Error != 0 ||
                !response.Logged ||
                response.LogType != "Warning" ||
                response.LoggedMessage != "[UnityBridge] Unitybridge_EditorLogHandler_Test")
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "EditorLogHandler should report logged warning message");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
