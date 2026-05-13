namespace ET.Test
{
    public class Unitybridge_BridgeVector2Message_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeVector2 value = BridgeVector2.Create();
            value.X = 1.25f;
            value.Y = 2.5f;

            BridgeVector2 roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.X, value.X) ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.Y, value.Y))
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgeVector2 should round-trip X and Y");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
