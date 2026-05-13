using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_TransformGetHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("TransformGetHandler"));
            target.transform.localPosition = new Vector3(1f, 2f, 3f);
            try
            {
                TransformGetRequest request = TransformGetRequest.Create();
                request.Path = target.name;

                IResponse rawResponse = await new UnityBridgeTransformGetHandler().Handle(request);
                if (rawResponse is not TransformGetResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "TransformGetHandler should return TransformGetResponse");
                }

                if (response.Error != 0 || response.Name != target.name || response.Transform == null)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "TransformGetHandler should return transform info for target");
                }

                if (!UnityBridgeHandlerTestSupport.NearlyEqual(response.Transform.LocalPosition.X, 1f))
                {
                    return UnityBridgeProtocolTestSupport.Fail(3, "TransformGetHandler should include local position data");
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
