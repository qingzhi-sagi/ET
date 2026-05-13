namespace ET.Test
{
    public class Unitybridge_SelectionAddRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SelectionAddRequest value = SelectionAddRequest.Create();
            value.RpcId = 3203;
            value.Path = "Root/Enemy";
            value.AssetPath = "Assets/Prefabs/Enemy.prefab";
            value.InstanceId = 3002;

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(SelectionAddRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            SelectionAddRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Path != value.Path ||
                roundTrip.AssetPath != value.AssetPath ||
                roundTrip.InstanceId != value.InstanceId)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "SelectionAddRequest should round-trip target fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
