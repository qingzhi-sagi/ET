using UnityEditor;

namespace ET.Test
{
    public class Unitybridge_ExitPlayModeHandlerNotInPlayModeError_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "ExitPlayMode error code test must run outside PlayMode");
            }

            UnityBridgeRequestEnvelope request = new()
            {
                RpcId = 73001,
                IdempotencyKey = nameof(Unitybridge_ExitPlayModeHandlerNotInPlayModeError_Test),
                TimeoutMs = 1000,
                CommandJson = UnityBridgeMongoJsonHelper.ToCommandJson(ExitPlay.Create())
            };

            IResponse rawResponse;
            using (UnityBridgeDeferredRuntime.EnterRequestScope(request))
            {
                rawResponse = await new UnityBridgeExitPlayModeHandler().Handle(ExitPlay.Create());
            }

            if (rawResponse == null)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "ExitPlayModeHandler should return an error response");
            }

            if (rawResponse.Error != UnityBridgeErrorCode.NotInPlayMode)
            {
                return UnityBridgeProtocolTestSupport.Fail(3, "ExitPlayModeHandler should preserve NotInPlayMode error code");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
