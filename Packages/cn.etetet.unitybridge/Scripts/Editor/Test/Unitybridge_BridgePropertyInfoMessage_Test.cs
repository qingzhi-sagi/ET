namespace ET.Test
{
    public class Unitybridge_BridgePropertyInfoMessage_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            BridgePropertyInfo value = BridgePropertyInfo.Create();
            value.Name = "m_LocalPosition";
            value.DisplayName = "Position";
            value.Type = "Vector3";
            value.StringValue = "raw";
            value.IntValue = 12;
            value.FloatValue = 3.5f;
            value.BoolValue = true;
            value.Vector2Value = BridgeVector2.Create();
            value.Vector2Value.X = 4.5f;
            value.Vector2Value.Y = 5.5f;
            value.Vector3Value = UnityBridgeProtocolTestSupport.CreateVector3(1f, 2f, 3f);
            value.ObjectReferencePath = "Assets/Prefabs/Player.prefab";
            value.ObjectReferenceType = "GameObject";
            value.IsArray = true;
            value.IsEditable = true;

            BridgePropertyInfo roundTrip = UnityBridgeProtocolTestSupport.RoundTrip(value);
            if (roundTrip == null ||
                roundTrip.Name != value.Name ||
                roundTrip.DisplayName != value.DisplayName ||
                roundTrip.Type != value.Type ||
                roundTrip.StringValue != value.StringValue ||
                roundTrip.IntValue != value.IntValue ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.FloatValue, value.FloatValue) ||
                roundTrip.BoolValue != value.BoolValue ||
                roundTrip.Vector2Value == null ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.Vector2Value.Y, value.Vector2Value.Y) ||
                roundTrip.Vector3Value == null ||
                !UnityBridgeProtocolTestSupport.NearlyEqual(roundTrip.Vector3Value.Z, value.Vector3Value.Z) ||
                roundTrip.ObjectReferencePath != value.ObjectReferencePath ||
                roundTrip.ObjectReferenceType != value.ObjectReferenceType ||
                roundTrip.IsArray != value.IsArray ||
                roundTrip.IsEditable != value.IsEditable)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "BridgePropertyInfo should round-trip property value fields");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
