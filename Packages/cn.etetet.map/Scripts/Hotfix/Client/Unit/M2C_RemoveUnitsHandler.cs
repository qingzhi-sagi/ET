namespace ET.Client
{
	[MessageHandler(SceneType.Current)]
	public class M2C_RemoveUnitsHandler: MessageHandler<Scene, M2C_RemoveUnits>
	{
		protected override async ETTask Run(Scene currentScene, M2C_RemoveUnits message)
		{	
			UnitComponent unitComponent = currentScene.GetComponent<UnitComponent>();
			foreach (long unitId in message.Units)
			{
				unitComponent.Remove(unitId);
			}

			await ETTask.CompletedTask;
		}
	}
}
