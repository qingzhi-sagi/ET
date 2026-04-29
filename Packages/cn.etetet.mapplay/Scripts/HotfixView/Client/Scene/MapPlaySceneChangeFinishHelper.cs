namespace ET.Client
{
    public static class MapPlaySceneChangeFinishHelper
    {
        public static async ETTask OnSceneChangeFinish(Scene scene)
        {
            var currentScene = scene.GetComponent<CurrentScenesComponent>().Scene;
            var yiuiRoot     = currentScene.GetComponent<YIUIRootComponent>();
            EntityRef<YIUIRootComponent> yiuiRootRef = yiuiRoot;
            await yiuiRoot.OpenPanelAsync<MainPanelComponent>();
            yiuiRoot = yiuiRootRef;
            await yiuiRoot.OpenPanelAsync<HUDPanelComponent>();
        }
    }
}
