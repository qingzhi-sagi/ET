namespace ET.Test
{
    public class Unitybridge_SceneNewRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SceneNewRequest value = SceneNewRequest.Create();
            value.RpcId = 3105;
            value.Setup = "empty";

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(SceneNewRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            SceneNewRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Setup != value.Setup)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "SceneNewRequest should round-trip setup field");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
