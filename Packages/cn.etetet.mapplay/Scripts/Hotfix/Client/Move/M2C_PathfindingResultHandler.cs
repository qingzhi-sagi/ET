namespace ET.Client
{
	[MessageHandler(SceneType.Current)]
	public class M2C_PathfindingResultHandler : MessageHandler<Scene, M2C_PathfindingResult>
	{
		protected override async ETTask Run(Scene currentScene, M2C_PathfindingResult message)
		{
			Unit unit = currentScene.GetComponent<UnitComponent>().Get(message.Id);

			float speed = unit.NumericComponent.GetAsFloat(NumericType.Speed);

			await unit.GetComponent<MoveComponent>().MoveToAsync(message.Points, speed, 200);
		}
	}
}
