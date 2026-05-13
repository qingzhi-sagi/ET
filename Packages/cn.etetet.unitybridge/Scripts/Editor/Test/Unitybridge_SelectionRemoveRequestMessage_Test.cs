namespace ET.Test
{
    public class Unitybridge_SelectionRemoveRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SelectionRemoveRequest value = SelectionRemoveRequest.Create();
            value.RpcId = 3204;
            value.Path = "Root/Enemy";
            value.AssetPath = "Assets/Prefabs/Enemy.prefab";
            value.InstanceId = 3002;

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(SelectionRemoveRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            SelectionRemoveRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Path != value.Path ||
                roundTrip.AssetPath != value.AssetPath ||
                roundTrip.InstanceId != value.InstanceId)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "SelectionRemoveRequest should round-trip target fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
