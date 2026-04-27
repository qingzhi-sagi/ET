namespace ET.Client
{
	[MessageHandler(SceneType.Current)]
	public class M2C_CreateMyUnitHandler: MessageHandler<Scene, M2C_CreateMyUnit>
	{
		protected override async ETTask Run(Scene currentScene, M2C_CreateMyUnit message)
		{
			// 通知场景切换协程继续往下走
			currentScene.Root().GetComponent<ObjectWait>().Notify(new Wait_CreateMyUnit() {Message = message});
			await ETTask.CompletedTask;
		}
	}
}
