namespace ET.Test
{
    public class Unitybridge_EditorGetStateRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            EditorGetStateRequest request = EditorGetStateRequest.Create();
            request.RpcId = 1001;

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(request, typeof(EditorGetStateRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            EditorGetStateRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(request);
            if (roundTrip == null || roundTrip.RpcId != request.RpcId)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "EditorGetStateRequest should round-trip RpcId");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
