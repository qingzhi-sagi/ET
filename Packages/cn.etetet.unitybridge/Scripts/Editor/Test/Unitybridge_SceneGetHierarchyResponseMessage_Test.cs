namespace ET.Test
{
    public class Unitybridge_SceneGetHierarchyResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SceneGetHierarchyResponse value = SceneGetHierarchyResponse.Create();
            value.RpcId = 3101;
            value.Error = 0;
            value.Message = "ok";
            value.SceneName = "Main";
            value.ScenePath = "Assets/Scenes/Main.unity";
            value.RootCount = 1;
            value.Roots.Add(UnityBridgeProtocolTestSupport.CreateSceneNode());

            SceneGetHierarchyResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.SceneName != value.SceneName ||
                roundTrip.ScenePath != value.ScenePath ||
                roundTrip.RootCount != value.RootCount ||
                roundTrip.Roots.Count != 1 ||
                roundTrip.Roots[0].Object == null ||
                roundTrip.Roots[0].Object.Path != value.Roots[0].Object.Path)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "SceneGetHierarchyResponse should round-trip hierarchy result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
