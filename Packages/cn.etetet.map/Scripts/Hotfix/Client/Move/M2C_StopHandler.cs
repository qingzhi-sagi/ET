using Unity.Mathematics;

namespace ET.Client
{
	[MessageHandler(SceneType.Current)]
	public class M2C_StopHandler : MessageHandler<Scene, M2C_Stop>
	{
		protected override async ETTask Run(Scene currentScene, M2C_Stop message)
		{
			Unit unit = currentScene.GetComponent<UnitComponent>().Get(message.Id);
			if (unit == null)
			{
				return;
			}

			MoveComponent moveComponent = unit.GetComponent<MoveComponent>();
			moveComponent.Stop(message.Error == 0);
			unit.Position = message.Position;
			unit.Rotation = message.Rotation;
			unit.GetComponent<ObjectWait>()?.Notify(new Wait_UnitStop() {Error = message.Error});
			await ETTask.CompletedTask;
		}
	}
}
