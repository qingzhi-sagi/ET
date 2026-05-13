using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_TransformResetHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("TransformResetHandler"));
            target.transform.localPosition = new Vector3(1f, 2f, 3f);
            target.transform.localEulerAngles = new Vector3(0f, 45f, 0f);
            target.transform.localScale = new Vector3(2f, 3f, 4f);
            try
            {
                TransformResetRequest request = TransformResetRequest.Create();
                request.Path = target.name;

                IResponse rawResponse = await new UnityBridgeTransformResetHandler().Handle(request);
                if (rawResponse is not TransformResetResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "TransformResetHandler should return TransformResetResponse");
                }

                if (response.Error != 0 ||
                    !UnityBridgeHandlerTestSupport.VectorNearlyEqual(target.transform.localPosition, 0f, 0f, 0f) ||
                    !UnityBridgeHandlerTestSupport.VectorNearlyEqual(target.transform.localScale, 1f, 1f, 1f))
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "TransformResetHandler should reset position, rotation, and scale by default");
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
