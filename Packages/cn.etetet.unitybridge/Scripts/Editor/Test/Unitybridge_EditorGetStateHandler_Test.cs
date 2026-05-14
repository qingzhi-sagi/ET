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

            IResponse rawPingResponse = await new UnityBridgePingHandler().Handle(Ping.Create());
            if (rawPingResponse is not PingResponse pingResponse)
            {
                return UnityBridgeProtocolTestSupport.Fail(3, "PingHandler should return PingResponse");
            }

            if (pingResponse.Error != 0 ||
                pingResponse.Time <= 0 ||
                pingResponse.IsCompiling != EditorApplication.isCompiling ||
                pingResponse.IsPlaying != EditorApplication.isPlaying ||
                pingResponse.IsPlayingOrWillChangePlaymode != EditorApplication.isPlayingOrWillChangePlaymode ||
                string.IsNullOrWhiteSpace(pingResponse.CodeMode) ||
                string.IsNullOrWhiteSpace(pingResponse.UnityVersion))
            {
                return UnityBridgeProtocolTestSupport.Fail(4, "PingHandler should mirror Unity editor state");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
