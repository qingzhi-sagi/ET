namespace ET.Test
{
    public class Unitybridge_DeferredHandlerRunContext_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            int started = 0;
            UnityBridgeDeferredContext deferred = UnityBridgeDeferredContext.CreateResume(1234);

            await deferred.Defer(() => ++started);
            if (started != 0)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "Deferred resume should not run the deferred start action again");
            }

            if (!deferred.IsResuming || deferred.StartedAt != 1234)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "Deferred resume context should expose resume state and start time");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
