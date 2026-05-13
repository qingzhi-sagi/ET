namespace ET.Test
{
    public class Unitybridge_InspectorGetPropertyResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;
            return UnityBridgeProtocolTestSupport.AssertResponseRoundTrip<InspectorGetPropertyResponse>();
        }
    }
}

