namespace ET.Client
{
    public static class CurrentSceneFactory
    {
        public static Scene Create(long id, string name, CurrentScenesComponent currentScenesComponent)
        {
            Scene currentScene = EntitySceneFactory.CreateScene(currentScenesComponent, id, SceneType.Current, name);
            currentScene.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            currentScenesComponent.Scene = currentScene;
            
            EventSystem.Instance.Publish(currentScene, new AfterCreateCurrentScene());
            return currentScene;
        }
    }
}
