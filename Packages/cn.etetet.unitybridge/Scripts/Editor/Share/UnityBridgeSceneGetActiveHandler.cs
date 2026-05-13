using UnityEngine.SceneManagement;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace ET
{
    internal sealed class UnityBridgeSceneGetActiveHandler : AUnityBridgeHandler<SceneGetActiveRequest, SceneGetActiveResponse>
    {
        protected override async ETTask<IResponse> Run(SceneGetActiveRequest command)
        {
            await ETTask.CompletedTask;

            UnityScene scene = SceneManager.GetActiveScene();
            SceneGetActiveResponse response = SceneGetActiveResponse.Create();
            response.SceneName = scene.name;
            response.ScenePath = scene.path;
            response.IsLoaded = scene.isLoaded;
            response.IsDirty = scene.isDirty;
            response.RootCount = scene.rootCount;
            return response;
        }
    }
}
