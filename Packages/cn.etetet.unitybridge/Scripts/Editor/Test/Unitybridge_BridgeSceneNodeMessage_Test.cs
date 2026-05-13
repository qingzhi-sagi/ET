namespace ET.Test
{
    public class Unitybridge_BridgeSceneNodeMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeSceneNode value = BridgeSceneNode.Create();
            value.Object = UnityBridgeProtocolTestSupport.CreateObjectInfo();
            BridgeSceneNode child = BridgeSceneNode.Create();
            child.Object = UnityBridgeProtocolTestSupport.CreateObjectInfo("Child", "Root/Player/Child", 3002);
            value.Children.Add(child);

            BridgeSceneNode roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.Object == null ||
                roundTrip.Object.Path != value.Object.Path ||
                roundTrip.Children.Count != 1 ||
                roundTrip.Children[0].Object == null ||
                roundTrip.Children[0].Object.Name != child.Object.Name)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgeSceneNode should round-trip object and children");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
