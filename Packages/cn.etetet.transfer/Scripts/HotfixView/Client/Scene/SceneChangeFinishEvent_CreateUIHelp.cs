namespace ET.Client
{
    [Event(SceneType.Client)]
    public class SceneChangeFinishEvent_CreateUIHelp: AEvent<Scene, SceneChangeFinish>
    {
        protected override async ETTask Run(Scene scene, SceneChangeFinish args)
        {
            await MapPlaySceneChangeFinishHelper.OnSceneChangeFinish(scene);
        }
    }
}
