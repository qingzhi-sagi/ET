namespace ET.Test
{
    public class Unitybridge_SelectionSetRequestMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            SelectionSetRequest value = SelectionSetRequest.Create();
            value.RpcId = 3202;
            value.Path = "Root/Player";
            value.AssetPath = "Assets/Prefabs/Player.prefab";
            value.InstanceId = 3001;
            value.InstanceIds.Add(3001);
            value.InstanceIds.Add(3002);

            string typeError = UnityBridgeProtocolTestSupport.AssertRequestType(value, typeof(SelectionSetRequest).FullName);
            if (typeError != null)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, typeError);
            }

            SelectionSetRequest roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.RpcId != value.RpcId ||
                roundTrip.Path != value.Path ||
                roundTrip.AssetPath != value.AssetPath ||
                roundTrip.InstanceId != value.InstanceId ||
                roundTrip.InstanceIds.Count != 2 ||
                roundTrip.InstanceIds[1] != value.InstanceIds[1])
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "SelectionSetRequest should round-trip target fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
