namespace ET.Test
{
    public class Unitybridge_SceneGetHierarchyRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SceneGetHierarchyRequest value = SceneGetHierarchyRequest.Create();
            value.RpcId = 3101;
            value.Depth = 3;
            value.IncludeInactive = true;

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(SceneGetHierarchyRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            SceneGetHierarchyRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Depth != value.Depth ||
                roundTrip.IncludeInactive != value.IncludeInactive)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "SceneGetHierarchyRequest should round-trip hierarchy fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
