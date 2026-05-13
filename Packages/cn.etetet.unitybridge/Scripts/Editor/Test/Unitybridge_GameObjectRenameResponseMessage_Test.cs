namespace ET.Test
{
    public class Unitybridge_GameObjectRenameResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;
            return UnityBridgeProtocolTestSupport.AssertResponseRoundTrip<GameObjectRenameResponse>();
        }
    }
}

