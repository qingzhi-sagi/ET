namespace ET.Client
{
    [MessageHandler(SceneType.Current)]
    public class M2C_BuffAddHandler: MessageHandler<Scene, M2C_BuffAdd>
    {
        protected override async ETTask Run(Scene currentScene, M2C_BuffAdd message)
        {
            Unit unit = currentScene.GetComponent<UnitComponent>().Get(message.UnitId);
            BuffHelper.CreateBuff(unit, message);
            await ETTask.CompletedTask;
        }
    }
}
