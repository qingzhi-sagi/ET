namespace ET.Client
{
    [MessageHandler(SceneType.Current)]
    public class M2C_UpdateCDHandler : MessageHandler<Scene, M2C_UpdateCD>
    {
        protected override async ETTask Run(Scene currentScene, M2C_UpdateCD message)
        {
            Unit unit = currentScene.GetComponent<UnitComponent>().Get(message.UnitId);
            if (unit == null)
            {
                return;
            }

            SpellComponent spellComponent = unit.GetComponent<SpellComponent>();
            spellComponent.UpdateCD(message.SpellConfigId, message.Time);
            await ETTask.CompletedTask;
        }
    }
}
