namespace ET.Test
{
    public class Unitybridge_AssetLoadRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            AssetLoadRequest value = AssetLoadRequest.Create();
            value.RpcId = 2104;
            value.AssetPath = "Assets/Prefabs/Player.prefab";

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(AssetLoadRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            AssetLoadRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.AssetPath != value.AssetPath)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "AssetLoadRequest should round-trip asset path");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
