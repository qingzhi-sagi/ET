namespace ET.Test
{
    public class Unitybridge_SceneSaveResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SceneSaveResponse value = SceneSaveResponse.Create();
            value.RpcId = 3104;
            value.Error = 0;
            value.Message = "ok";
            value.SceneName = "Main";
            value.ScenePath = "Assets/Scenes/NewScene.unity";
            value.Saved = true;

            SceneSaveResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.SceneName != value.SceneName ||
                roundTrip.ScenePath != value.ScenePath ||
                roundTrip.Saved != value.Saved)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "SceneSaveResponse should round-trip save result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
