namespace ET.Test
{
    public class Unitybridge_BatchExecuteHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            BatchExecuteRequest request = BatchExecuteRequest.Create();
            request.Commands.Add(UnityBridgeMongoJsonHelper.ToCommandJson(Ping.Create()));

            IResponse rawResponse = await new UnityBridgeBatchExecuteHandler().Handle(request);
            if (rawResponse is not BatchExecuteResponse response)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BatchExecuteHandler should return BatchExecuteResponse");
            }

            if (response.Error != 0 ||
                !response.Completed ||
                response.Count != 1 ||
                response.Failed != 0 ||
                response.Results.Count != 1 ||
                response.Results[0].Name != nameof(Ping))
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "BatchExecuteHandler should dispatch child command and collect result");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
