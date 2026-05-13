using UnityEditor;

namespace ET.Test
{
    public class Unitybridge_EditorGetStateHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            EditorGetStateRequest request = EditorGetStateRequest.Create();

            IResponse rawResponse = await new UnityBridgeEditorGetStateHandler().Handle(request);
            if (rawResponse is not EditorGetStateResponse response)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "EditorGetStateHandler should return EditorGetStateResponse");
            }

            if (response.Error != 0 ||
                response.IsPlaying != EditorApplication.isPlaying ||
                response.IsPaused != EditorApplication.isPaused ||
                response.IsCompiling != EditorApplication.isCompiling ||
                string.IsNullOrWhiteSpace(response.ApplicationPath))
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "EditorGetStateHandler should mirror Unity editor state");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
