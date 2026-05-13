using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_InspectorSetPropertiesHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("InspectorSetPropertiesHandler"));
            try
            {
                BridgePropertyInfo localScale = BridgePropertyInfo.Create();
                localScale.PropertyPath = "m_LocalScale";
                localScale.Vector3Value = UnityBridgeHandlerTestSupport.CreateVector3(3f, 3f, 3f);

                InspectorSetPropertiesRequest request = InspectorSetPropertiesRequest.Create();
                request.Path = target.name;
                request.ComponentName = nameof(Transform);
                request.Values.Add(localScale);

                IResponse rawResponse = await new UnityBridgeInspectorSetPropertiesHandler().Handle(request);
                if (rawResponse is not InspectorSetPropertiesResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "InspectorSetPropertiesHandler should return InspectorSetPropertiesResponse");
                }

                if (response.Error != 0 || !response.Changed || !UnityBridgeHandlerTestSupport.VectorNearlyEqual(target.transform.localScale, 3f, 3f, 3f))
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "InspectorSetPropertiesHandler should update serialized properties");
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
