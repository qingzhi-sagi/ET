using Unity.Mathematics;

namespace ET.Client
{
	[MessageHandler(SceneType.Current)]
	public class M2C_TurnHandler : MessageHandler<Scene, M2C_Turn>
	{
		protected override async ETTask Run(Scene currentScene, M2C_Turn message)
		{
			Unit unit = currentScene.GetComponent<UnitComponent>().Get(message.UnitId);
			if (unit == null)
			{
				return;
			}

			TurnComponent turnComponent = unit.GetComponent<TurnComponent>();
			turnComponent.Turn(message.Rotation, message.TurnTime);
			await ETTask.CompletedTask;
		}
	}
}
