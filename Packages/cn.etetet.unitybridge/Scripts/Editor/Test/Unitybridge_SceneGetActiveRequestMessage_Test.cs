namespace ET.Test
{
    public class Unitybridge_SceneGetActiveRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SceneGetActiveRequest value = SceneGetActiveRequest.Create();
            value.RpcId = 3102;

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(SceneGetActiveRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            SceneGetActiveRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null || roundTrip.RpcId != value.RpcId)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "SceneGetActiveRequest should round-trip rpc id");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
