namespace ET.Test
{
    public class Unitybridge_UnityTestRunHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            UnityTestRunRequest request = UnityTestRunRequest.Create();
            request.Name = "^Unitybridge_UnityTestRunRequestMessage_Test$";

            IResponse rawResponse = await new UnityBridgeUnityTestRunHandler().Handle(request);
            if (rawResponse is not UnityTestRunResponse response)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "UnityTestRunHandler should return UnityTestRunResponse");
            }

            if (response.Error != 0 ||
                response.Matched != 1 ||
                response.Passed != 1 ||
                response.Failed != 0 ||
                response.Results.Count != 1 ||
                response.Results[0].Name != "Unitybridge_UnityTestRunRequestMessage_Test")
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "UnityTestRunHandler should execute matching test by name regex");
            }

            UnityTestRunRequest sharedDispatcherRequest = UnityTestRunRequest.Create();
            sharedDispatcherRequest.Name = "^Unitybridge_UnityTestRunReceivesArgs_Test$";

            IResponse sharedDispatcherRawResponse = await new UnityBridgeUnityTestRunHandler().Handle(sharedDispatcherRequest);
            if (sharedDispatcherRawResponse is not UnityTestRunResponse sharedDispatcherResponse)
            {
                return UnityBridgeProtocolTestSupport.Fail(3, "UnityTestRunHandler should return UnityTestRunResponse for shared dispatcher tests");
            }

            if (sharedDispatcherResponse.Error != 0 ||
                sharedDispatcherResponse.Matched != 1 ||
                sharedDispatcherResponse.Passed != 1 ||
                sharedDispatcherResponse.Failed != 0 ||
                sharedDispatcherResponse.Results.Count != 1 ||
                sharedDispatcherResponse.Results[0].Name != "Unitybridge_UnityTestRunReceivesArgs_Test")
            {
                return UnityBridgeProtocolTestSupport.Fail(4, "UnityTestRunHandler should execute tests discovered by TestDispatcher");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
