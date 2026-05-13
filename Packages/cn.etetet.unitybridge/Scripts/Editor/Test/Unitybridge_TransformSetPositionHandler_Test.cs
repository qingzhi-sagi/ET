using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_TransformSetPositionHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject target = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("TransformSetPositionHandler"));
            try
            {
                TransformSetPositionRequest request = TransformSetPositionRequest.Create();
                request.Path = target.name;
                request.Local = true;
                request.Position = UnityBridgeHandlerTestSupport.CreateVector3(4f, 5f, 6f);

                IResponse rawResponse = await new UnityBridgeTransformSetPositionHandler().Handle(request);
                if (rawResponse is not TransformSetPositionResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "TransformSetPositionHandler should return TransformSetPositionResponse");
                }

                if (response.Error != 0 || !UnityBridgeHandlerTestSupport.VectorNearlyEqual(target.transform.localPosition, 4f, 5f, 6f))
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "TransformSetPositionHandler should update local position");
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
