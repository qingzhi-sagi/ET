namespace ET.Test
{
    public class Unitybridge_BridgeObjectInfoMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeObjectInfo value = UnityBridgeProtocolTestSupport.CreateObjectInfo();
            BridgeObjectInfo roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.Name != value.Name ||
                roundTrip.Path != value.Path ||
                roundTrip.InstanceId != value.InstanceId ||
                roundTrip.ActiveSelf != value.ActiveSelf ||
                roundTrip.ActiveInHierarchy != value.ActiveInHierarchy ||
                roundTrip.Tag != value.Tag ||
                roundTrip.LayerName != value.LayerName ||
                roundTrip.Layer != value.Layer ||
                roundTrip.Transform == null ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.Transform.LocalPosition.X, value.Transform.LocalPosition.X) ||
                roundTrip.Components.Count != 1 ||
                roundTrip.Components[0].TypeName != value.Components[0].TypeName)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgeObjectInfo should round-trip object, transform, and component fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
