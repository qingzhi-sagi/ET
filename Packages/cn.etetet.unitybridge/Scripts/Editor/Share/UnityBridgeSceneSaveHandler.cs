using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace ET
{
    internal sealed class UnityBridgeSceneSaveHandler : AUnityBridgeHandler<SceneSaveRequest, SceneSaveResponse>
    {
        protected override async ETTask<IResponse> Run(SceneSaveRequest command)
        {
            await ETTask.CompletedTask;

            UnityScene scene = SceneManager.GetActiveScene();
            bool saved;
            if (string.IsNullOrWhiteSpace(command.SaveAs))
            {
                saved = EditorSceneManager.SaveOpenScenes();
                scene = SceneManager.GetActiveScene();
            }
            else
            {
                saved = EditorSceneManager.SaveScene(scene, command.SaveAs.Trim());
                scene = SceneManager.GetActiveScene();
            }

            SceneSaveResponse response = SceneSaveResponse.Create();
            response.SceneName = scene.name;
            response.ScenePath = scene.path;
            response.Saved = saved;
            return response;
        }
    }
}
