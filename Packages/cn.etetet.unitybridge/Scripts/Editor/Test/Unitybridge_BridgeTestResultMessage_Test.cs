namespace ET.Test
{
    public class Unitybridge_BridgeTestResultMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeTestResult value = BridgeTestResult.Create();
            value.Name = "Unitybridge_AssetFindRequestMessage_Test";
            value.FullName = "ET.Test.Unitybridge_AssetFindRequestMessage_Test";
            value.Passed = true;
            value.Error = 0;
            value.Message = "success";
            value.DurationMs = 12;

            BridgeTestResult roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.Name != value.Name ||
                roundTrip.FullName != value.FullName ||
                roundTrip.Passed != value.Passed ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.DurationMs != value.DurationMs)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgeTestResult should round-trip test result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
