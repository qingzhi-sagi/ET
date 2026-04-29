namespace ET.Client
{
    [Event(SceneType.Client)]
    public class SceneChangeFinish_CreateMainUI: AEvent<Scene, SceneChangeFinish>
    {
        protected override async ETTask Run(Scene scene, SceneChangeFinish args)
        {
            Scene currentScene = scene.GetComponent<CurrentScenesComponent>().Scene;
            YIUIRootComponent yiuiRoot = currentScene.GetComponent<YIUIRootComponent>();
            EntityRef<YIUIRootComponent> yiuiRootRef = yiuiRoot;
            await yiuiRoot.OpenPanelAsync<MainPanelComponent>();
            yiuiRoot = yiuiRootRef;
            await yiuiRoot.OpenPanelAsync<HUDPanelComponent>();
        }
    }
}
