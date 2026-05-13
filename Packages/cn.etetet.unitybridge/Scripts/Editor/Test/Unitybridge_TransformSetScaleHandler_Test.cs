using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_TransformSetScaleHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("TransformSetScaleHandler"));
            try
            {
                TransformSetScaleRequest request = TransformSetScaleRequest.Create();
                request.Path = target.name;
                request.UseUniform = true;
                request.Uniform = 2.5f;

                IResponse rawResponse = await new UnityBridgeTransformSetScaleHandler().Handle(request);
                if (rawResponse is not TransformSetScaleResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "TransformSetScaleHandler should return TransformSetScaleResponse");
                }

                if (response.Error != 0 || !UnityBridgeHandlerTestSupport.VectorNearlyEqual(target.transform.localScale, 2.5f, 2.5f, 2.5f))
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "TransformSetScaleHandler should update local scale");
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
