using UnityEngine.SceneManagement;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace ET.Test
{
    public class Unitybridge_SceneGetActiveHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            SceneGetActiveRequest request = SceneGetActiveRequest.Create();

            IResponse rawResponse = await new UnityBridgeSceneGetActiveHandler().Handle(request);
            if (rawResponse is not SceneGetActiveResponse response)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "SceneGetActiveHandler should return SceneGetActiveResponse");
            }

            UnityScene scene = SceneManager.GetActiveScene();
            if (response.Error != 0 ||
                response.SceneName != scene.name ||
                response.ScenePath != scene.path ||
                response.IsLoaded != scene.isLoaded ||
                response.RootCount != scene.rootCount)
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "SceneGetActiveHandler should mirror active scene state");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
