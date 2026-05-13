using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_TransformLookAtHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("TransformLookAtHandler"));
            target.transform.position = Vector3.zero;
            try
            {
                TransformLookAtRequest request = TransformLookAtRequest.Create();
                request.Path = target.name;
                request.HasTargetPosition = true;
                request.TargetPosition = UnityBridgeHandlerTestSupport.CreateVector3(0f, 0f, 10f);

                IResponse rawResponse = await new UnityBridgeTransformLookAtHandler().Handle(request);
                if (rawResponse is not TransformLookAtResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "TransformLookAtHandler should return TransformLookAtResponse");
                }

                if (response.Error != 0 || Vector3.Dot(target.transform.forward, Vector3.forward) < 0.99f)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "TransformLookAtHandler should rotate target toward position");
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
