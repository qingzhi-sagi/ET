namespace ET.Test
{
    public class Unitybridge_SceneGetActiveResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SceneGetActiveResponse value = SceneGetActiveResponse.Create();
            value.RpcId = 3102;
            value.Error = 0;
            value.Message = "ok";
            value.SceneName = "Main";
            value.ScenePath = "Assets/Scenes/Main.unity";
            value.IsLoaded = true;
            value.IsDirty = false;
            value.RootCount = 2;

            SceneGetActiveResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.SceneName != value.SceneName ||
                roundTrip.ScenePath != value.ScenePath ||
                roundTrip.IsLoaded != value.IsLoaded ||
                roundTrip.IsDirty != value.IsDirty ||
                roundTrip.RootCount != value.RootCount)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "SceneGetActiveResponse should round-trip active scene fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
