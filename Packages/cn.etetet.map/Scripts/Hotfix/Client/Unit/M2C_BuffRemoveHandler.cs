namespace ET.Client
{
    [MessageHandler(SceneType.Current)]
    public class M2C_BuffRemoveHandler: MessageHandler<Scene, M2C_BuffRemove>
    {
        protected override async ETTask Run(Scene currentScene, M2C_BuffRemove message)
        {
            Unit unit = currentScene.GetComponent<UnitComponent>().Get(message.UnitId);
            BuffHelper.RemoveBuff(unit, message.BuffId, (BuffFlags)message.RemoveType);
            await ETTask.CompletedTask;
        }
    }
}
