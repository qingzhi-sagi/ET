namespace ET.Test
{
    public class Unitybridge_AssetImportRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            AssetImportRequest value = AssetImportRequest.Create();
            value.RpcId = 2105;
            value.AssetPath = "Assets/Textures/Icon.png";
            value.ForceUpdate = true;

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(AssetImportRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            AssetImportRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.AssetPath != value.AssetPath ||
                roundTrip.ForceUpdate != value.ForceUpdate)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "AssetImportRequest should round-trip import fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
