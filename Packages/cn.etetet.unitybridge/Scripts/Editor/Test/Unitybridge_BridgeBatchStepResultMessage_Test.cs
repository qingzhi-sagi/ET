namespace ET.Test
{
    public class Unitybridge_BridgeBatchStepResultMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgeBatchStepResult value = BridgeBatchStepResult.Create();
            value.Name = "find";
            value.Command = "AssetFindRequest";
            value.Error = 0;
            value.Message = "ok";

            BridgeBatchStepResult roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.Name != value.Name ||
                roundTrip.Command != value.Command ||
                roundTrip.Error != value.Error ||
                roundTrip.Message != value.Message)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgeBatchStepResult should round-trip batch result fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
