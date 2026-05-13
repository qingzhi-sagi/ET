namespace ET.Test
{
    public class Unitybridge_SceneLoadRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SceneLoadRequest value = SceneLoadRequest.Create();
            value.RpcId = 3103;
            value.ScenePath = "Assets/Scenes/Main.unity";
            value.Mode = "additive";

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(SceneLoadRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            SceneLoadRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.ScenePath != value.ScenePath ||
                roundTrip.Mode != value.Mode)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "SceneLoadRequest should round-trip load fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
