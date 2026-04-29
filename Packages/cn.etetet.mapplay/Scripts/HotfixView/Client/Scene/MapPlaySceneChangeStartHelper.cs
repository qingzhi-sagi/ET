using System;
using UnityEngine.SceneManagement;

namespace ET.Client
{
    public static class MapPlaySceneChangeStartHelper
    {
        public static async ETTask OnSceneChangeStart(Scene root, bool changeScene)
        {
            try
            {
                EntityRef<Scene> rootRef = root;
                EntityRef<CurrentScenesComponent> currentScenesComponentRef = root.GetComponent<CurrentScenesComponent>();
                CurrentScenesComponent currentScenesComponent = currentScenesComponentRef;
                using var _ = await root.CoroutineLockComponent.Wait(CoroutineLockType.SceneChange, 0);

                root = rootRef;
                currentScenesComponent = currentScenesComponentRef;
                Scene currentScene = currentScenesComponent.Scene;
                EntityRef<Scene> currentSceneRef = currentScene;
                // 地图资源相同,则不创建Loading界面,也不需要重新加载地图
                if (changeScene)
                {
                    await root.YIUIRoot().OpenPanelAsync<LoadingPanelComponent>();

                    root = rootRef;
                    currentScenesComponent = currentScenesComponentRef;
                    currentScenesComponent.Progress = 0;

                    currentScene = currentSceneRef;
                    ResourcesLoaderComponent resourcesLoaderComponent = currentScene.GetComponent<ResourcesLoaderComponent>();

                    MapConfig mapConfig = root.Fiber().GetSingleton<MapConfigCategory>().GetByName(currentScene.Name.GetSceneConfigName());
                    // 加载场景资源
                    await resourcesLoaderComponent.LoadSceneAsync(mapConfig.MapResName, LoadSceneMode.Single,
                        (progress) =>
                        {
                            CurrentScenesComponent currentScenes = currentScenesComponentRef;
                            currentScenes.Progress = (int)progress * 99f;
                        });
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

        }
    }
}
