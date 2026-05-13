namespace ET.Test
{
    public class Unitybridge_BridgeQuaternionMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeQuaternion value = UnityBridgeProtocolTestSupport.CreateQuaternion(0.1f, 0.2f, 0.3f, 0.4f);
            BridgeQuaternion roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.X, value.X) ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.Y, value.Y) ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.Z, value.Z) ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.W, value.W))
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgeQuaternion should round-trip X, Y, Z, and W");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
