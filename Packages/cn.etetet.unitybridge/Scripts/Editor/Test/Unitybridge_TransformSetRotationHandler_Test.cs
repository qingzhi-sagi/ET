using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_TransformSetRotationHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("TransformSetRotationHandler"));
            try
            {
                TransformSetRotationRequest request = TransformSetRotationRequest.Create();
                request.Path = target.name;
                request.Local = true;
                request.EulerAngles = UnityBridgeHandlerTestSupport.CreateVector3(0f, 90f, 0f);

                IResponse rawResponse = await new UnityBridgeTransformSetRotationHandler().Handle(request);
                if (rawResponse is not TransformSetRotationResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "TransformSetRotationHandler should return TransformSetRotationResponse");
                }

                if (response.Error != 0 || !UnityBridgeHandlerTestSupport.NearlyEqual(target.transform.localEulerAngles.y, 90f))
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "TransformSetRotationHandler should update local rotation");
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
