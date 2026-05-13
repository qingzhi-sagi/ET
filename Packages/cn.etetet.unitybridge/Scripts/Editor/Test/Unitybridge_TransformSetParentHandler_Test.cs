using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_TransformSetParentHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject parent = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("TransformSetParentParent"));
            GameObject child = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("TransformSetParentChild"));
            try
            {
                TransformSetParentRequest request = TransformSetParentRequest.Create();
                request.Path = child.name;
                request.ParentPath = parent.name;

                IResponse rawResponse = await new UnityBridgeTransformSetParentHandler().Handle(request);
                if (rawResponse is not TransformSetParentResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "TransformSetParentHandler should return TransformSetParentResponse");
                }

                if (response.Error != 0 || child.transform.parent != parent.transform || response.ParentPath != parent.name)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "TransformSetParentHandler should parent the target transform");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                UnityBridgeHandlerTestSupport.DestroyObjects(child, parent);
            }
        }
    }
}
