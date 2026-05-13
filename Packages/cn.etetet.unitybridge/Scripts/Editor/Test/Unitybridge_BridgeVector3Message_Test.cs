namespace ET.Test
{
    public class Unitybridge_BridgeVector3Message_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeVector3 value = UnityBridgeProtocolTestSupport.CreateVector3(1.25f, 2.5f, 3.75f);
            BridgeVector3 roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.X, value.X) ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.Y, value.Y) ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.Z, value.Z))
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgeVector3 should round-trip X, Y, and Z");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
