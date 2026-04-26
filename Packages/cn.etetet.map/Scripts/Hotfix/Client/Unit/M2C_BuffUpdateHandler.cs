namespace ET.Client
{
    [MessageHandler(SceneType.Current)]
    public class M2C_BuffUpdateHandler: MessageHandler<Scene, M2C_BuffUpdate>
    {
        protected override async ETTask Run(Scene currentScene, M2C_BuffUpdate message)
        {
            Unit unit = currentScene.GetComponent<UnitComponent>().Get(message.UnitId);
            Buff buff = unit.GetComponent<BuffComponent>().GetChild<Buff>(message.BuffId);
            BuffHelper.UpdateBuff(buff, message.ExpireTime, message.TickTime, message.Stack);
            await ETTask.CompletedTask;
        }
    }
}
