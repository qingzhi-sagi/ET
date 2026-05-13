namespace ET.Test
{
    public class Unitybridge_UnityTestRunResponseMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeTestResult result = BridgeTestResult.Create();
            result.Name = "Unitybridge_UnityTestRunRequestMessage_Test";
            result.FullName = "ET.Test.Unitybridge_UnityTestRunRequestMessage_Test";
            result.Passed = true;
            result.Error = 0;
            result.Message = "success";
            result.DurationMs = 2;

            UnityTestRunResponse value = UnityTestRunResponse.Create();
            value.RpcId = 3301;
            value.Error = 0;
            value.Message = "ok";
            value.Name = "Unitybridge_UnityTestRunRequestMessage_Test";
            value.Matched = 1;
            value.Passed = 1;
            value.Failed = 0;
            value.DurationMs = 3;
            value.Results.Add(result);

            UnityTestRunResponse roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message ||
                roundTrip.Name != value.Name ||
                roundTrip.Matched != value.Matched ||
                roundTrip.Passed != value.Passed ||
                roundTrip.Failed != value.Failed ||
                roundTrip.DurationMs != value.DurationMs ||
                roundTrip.Results.Count != 1 ||
                roundTrip.Results[0].Name != result.Name)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "UnityTestRunResponse should round-trip summary and results");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
