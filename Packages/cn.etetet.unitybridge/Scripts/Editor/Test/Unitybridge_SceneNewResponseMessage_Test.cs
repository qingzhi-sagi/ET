namespace ET.Test
{
    public class Unitybridge_SceneNewResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SceneNewResponse value = SceneNewResponse.Create();
            value.RpcId = 3105;
            value.Error = 0;
            value.Message = "ok";
            value.SceneName = "Untitled";
            value.ScenePath = string.Empty;
            value.Created = true;

            SceneNewResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.SceneName != value.SceneName ||
                roundTrip.ScenePath != value.ScenePath ||
                roundTrip.Created != value.Created)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "SceneNewResponse should round-trip new scene result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
