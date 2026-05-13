namespace ET.Test
{
    public class Unitybridge_GameObjectFindResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;
            return UnityBridgeProtocolTestSupport.AssertResponseRoundTrip<GameObjectFindResponse>();
        }
    }
}

