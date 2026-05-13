using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_InspectorGetPropertyHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("InspectorGetPropertyHandler"));
            target.transform.localPosition = new Vector3(7f, 8f, 9f);
            try
            {
                InspectorGetPropertyRequest request = InspectorGetPropertyRequest.Create();
                request.Path = target.name;
                request.ComponentName = nameof(Transform);
                request.PropertyName = "m_LocalPosition";

                IResponse rawResponse = await new UnityBridgeInspectorGetPropertyHandler().Handle(request);
                if (rawResponse is not InspectorGetPropertyResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "InspectorGetPropertyHandler should return InspectorGetPropertyResponse");
                }

                if (response.Error != 0 || response.Property == null || response.Property.PropertyPath != "m_LocalPosition")
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "InspectorGetPropertyHandler should return requested serialized property");
                }

                if (response.Property.Vector3Value == null || !UnityBridgeHandlerTestSupport.NearlyEqual(response.Property.Vector3Value.X, 7f))
                {
                    return UnityBridgeProtocolTestSupport.Fail(3, "InspectorGetPropertyHandler should include serialized property value");
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
