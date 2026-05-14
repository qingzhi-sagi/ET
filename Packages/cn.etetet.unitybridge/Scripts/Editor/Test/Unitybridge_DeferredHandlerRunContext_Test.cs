namespace ET.Test
{
    public class Unitybridge_DeferredHandlerRunContext_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            UnityBridgeDeferredContext resume = UnityBridgeDeferredContext.CreateResume(1234);
            if (!resume.IsResuming || resume.StartedAt != 1234)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "Deferred resume context should expose resume state and start time");
            }

            UnityBridgeDeferredContext start = UnityBridgeDeferredContext.CreateStart();
            if (start.IsResuming || start.StartedAt != 0)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "Deferred start context should expose start state");
            }

            try
            {
                start.Started<IResponse>();
                return UnityBridgeProtocolTestSupport.Fail(3, "Deferred start should throw started signal");
            }
            catch (UnityBridgeDeferredStartedException)
            {
            }

            try
            {
                resume.NotReady<IResponse>();
                return UnityBridgeProtocolTestSupport.Fail(4, "Deferred resume not-ready should throw not-ready signal");
            }
            catch (UnityBridgeDeferredNotReadyException)
            {
            }

            return ErrorCode.ERR_Success;
        }
    }
}
