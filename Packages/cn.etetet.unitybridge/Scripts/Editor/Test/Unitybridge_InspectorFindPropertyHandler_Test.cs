using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_InspectorFindPropertyHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("InspectorFindPropertyHandler"));
            try
            {
                InspectorFindPropertyRequest request = InspectorFindPropertyRequest.Create();
                request.Path = target.name;
                request.ComponentName = nameof(Transform);
                request.Keyword = "LocalPosition";

                IResponse rawResponse = await new UnityBridgeInspectorFindPropertyHandler().Handle(request);
                if (rawResponse is not InspectorFindPropertyResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "InspectorFindPropertyHandler should return InspectorFindPropertyResponse");
                }

                if (response.Error != 0 || response.Keyword != request.Keyword || response.Count == 0)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "InspectorFindPropertyHandler should find matching serialized properties");
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
