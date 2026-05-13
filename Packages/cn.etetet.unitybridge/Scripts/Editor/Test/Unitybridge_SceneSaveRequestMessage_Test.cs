namespace ET.Test
{
    public class Unitybridge_SceneSaveRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SceneSaveRequest value = SceneSaveRequest.Create();
            value.RpcId = 3104;
            value.SaveAs = "Assets/Scenes/NewScene.unity";

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(SceneSaveRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            SceneSaveRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.SaveAs != value.SaveAs)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "SceneSaveRequest should round-trip save fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
