namespace ET.Test
{
    public class Unitybridge_BridgeTransformInfoMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeTransformInfo value = UnityBridgeProtocolTestSupport.CreateTransformInfo();
            BridgeTransformInfo roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.LocalPosition == null ||
                roundTrip.LocalEulerAngles == null ||
                roundTrip.LocalRotation == null ||
                roundTrip.LocalScale == null ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.LocalPosition.X, value.LocalPosition.X) ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.LocalEulerAngles.Y, value.LocalEulerAngles.Y) ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.LocalRotation.W, value.LocalRotation.W) ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.LocalScale.Z, value.LocalScale.Z) ||
                roundTrip.ParentPath != value.ParentPath ||
                roundTrip.SiblingIndex != value.SiblingIndex)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgeTransformInfo should round-trip nested transform fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
