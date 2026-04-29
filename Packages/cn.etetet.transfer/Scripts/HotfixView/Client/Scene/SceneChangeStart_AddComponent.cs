namespace ET.Client
{
    [Event(SceneType.Client)]
    public class SceneChangeStart_AddComponent: AEvent<Scene, SceneChangeStart>
    {
        protected override async ETTask Run(Scene root, SceneChangeStart args)
        {
            await MapPlaySceneChangeStartHelper.OnSceneChangeStart(root, args.ChangeScene);
        }
    }
}
