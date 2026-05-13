using UnityEditor;

namespace ET.Test
{
    public class Unitybridge_EditorPauseHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            bool original = EditorApplication.isPaused;
            try
            {
                EditorPauseRequest request = EditorPauseRequest.Create();
                request.Pause = true;

                IResponse rawResponse = await new UnityBridgeEditorPauseHandler().Handle(request);
                if (rawResponse is not EditorPauseResponse response)
                {
                    return UnityBridgeProtocolTestSupport.Fail(1, "EditorPauseHandler should return EditorPauseResponse");
                }

                if (response.Error != 0 || !response.IsPaused || !EditorApplication.isPaused)
                {
                    return UnityBridgeProtocolTestSupport.Fail(2, "EditorPauseHandler should set editor pause state");
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                EditorApplication.isPaused = original;
            }
        }
    }
}
