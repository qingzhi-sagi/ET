namespace ET.Test
{
    public class Unitybridge_GameObjectGetInfoRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;
            return UnityBridgeProtocolTestSupport.AssertRequestRoundTrip<GameObjectGetInfoRequest>();
        }
    }
}

