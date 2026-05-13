using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_TransformSetSiblingIndexHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            GameObject parent = UnityBridgeHandlerTestSupport.CreateSceneObject(UnityBridgeHandlerTestSupport.UniqueName("TransformSiblingParent"));
            GameObject first = UnityBridgeHandlerTestSupport.CreateSceneObject("First", parent.transform);
            GameObject second = UnityBridgeHandlerTestSupport.CreateSceneObject("Second", parent.transform);
            try
            {
                TransformSetSiblingIndexRequest request = TransformSetSiblingIndexRequest.Create();
                request.Path = UnityBridgeHandlerTestSupport.GetPath(second);
                request.Index = 0;

                IResponse rawResponse = await new UnityBridgeTransformSetSiblingIndexHandler().Handle(request);
                if (rawResponse is not TransformSetSiblingIndexResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "TransformSetSiblingIndexHandler should return TransformSetSiblingIndexResponse");
                }

                if (response.Error != 0 || second.transform.GetSiblingIndex() != 0 || first.transform.GetSiblingIndex() != 1)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "TransformSetSiblingIndexHandler should reorder sibling index");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                UnityBridgeHandlerTestSupport.DestroyObjects(second, first, parent);
            }
        }
    }
}
