using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_InspectorSetPropertyHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("InspectorSetPropertyHandler"));
            try
            {
                BridgePropertyInfo value = BridgePropertyInfo.Create();
                value.Vector3Value = UnityBridgeHandlerTestSupport.CreateVector3(2f, 4f, 6f);

                InspectorSetPropertyRequest request = InspectorSetPropertyRequest.Create();
                request.Path = target.name;
                request.ComponentName = nameof(Transform);
                request.PropertyName = "m_LocalPosition";
                request.Value = value;

                IResponse rawResponse = await new UnityBridgeInspectorSetPropertyHandler().Handle(request);
                if (rawResponse is not InspectorSetPropertyResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "InspectorSetPropertyHandler should return InspectorSetPropertyResponse");
                }

                if (response.Error != 0 || !response.Changed || !UnityBridgeHandlerTestSupport.VectorNearlyEqual(target.transform.localPosition, 2f, 4f, 6f))
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "InspectorSetPropertyHandler should update serialized property value");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                UnityBridgeHandlerTestSupport.DestroyObjects(target);
            }
        }
    }
}
