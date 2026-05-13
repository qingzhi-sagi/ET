namespace ET.Test
{
    public class Unitybridge_BridgeComponentInfoMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeComponentInfo value = BridgeComponentInfo.Create();
            value.TypeName = "Transform";
            value.FullTypeName = "UnityEngine.Transform";
            value.ComponentIndex = 1;
            value.InstanceId = 4001;
            value.Enabled = true;

            BridgeComponentInfo roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.TypeName != value.TypeName ||
                roundTrip.FullTypeName != value.FullTypeName ||
                roundTrip.ComponentIndex != value.ComponentIndex ||
                roundTrip.InstanceId != value.InstanceId ||
                roundTrip.Enabled != value.Enabled)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgeComponentInfo should round-trip component fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
