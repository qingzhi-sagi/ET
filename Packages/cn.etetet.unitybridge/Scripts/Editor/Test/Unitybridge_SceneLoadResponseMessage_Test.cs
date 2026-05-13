namespace ET.Test
{
    public class Unitybridge_SceneLoadResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SceneLoadResponse value = SceneLoadResponse.Create();
            value.RpcId = 3103;
            value.Error = 0;
            value.Message = "ok";
            value.SceneName = "Main";
            value.ScenePath = "Assets/Scenes/Main.unity";
            value.Loaded = true;

            SceneLoadResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.SceneName != value.SceneName ||
                roundTrip.ScenePath != value.ScenePath ||
                roundTrip.Loaded != value.Loaded)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "SceneLoadResponse should round-trip load result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
