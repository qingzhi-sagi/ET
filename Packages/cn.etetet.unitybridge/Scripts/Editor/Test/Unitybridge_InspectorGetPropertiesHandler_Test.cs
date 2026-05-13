using System.Linq;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_InspectorGetPropertiesHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("InspectorGetPropertiesHandler"));
            target.transform.localPosition = new Vector3(1f, 2f, 3f);
            try
            {
                InspectorGetPropertiesRequest request = InspectorGetPropertiesRequest.Create();
                request.Path = target.name;
                request.ComponentName = nameof(Transform);
                request.IncludeChildren = true;

                IResponse rawResponse = await new UnityBridgeInspectorGetPropertiesHandler().Handle(request);
                if (rawResponse is not InspectorGetPropertiesResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "InspectorGetPropertiesHandler should return InspectorGetPropertiesResponse");
                }

                if (response.Error != 0 || response.ComponentName != nameof(Transform) || response.Properties.Count == 0)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "InspectorGetPropertiesHandler should return serialized Transform properties");
                }

                if (!response.Properties.Any(property => property.PropertyPath == "m_LocalPosition"))
                {
                    return UnityBridgeProtocolTestSupport.Fail(3, "InspectorGetPropertiesHandler should include local position property");
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
