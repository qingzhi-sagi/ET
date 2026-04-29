namespace ET.Server
{
	[MessageHandler(SceneType.Map)]
	public class G2Map_EnterMapHandler : MessageHandler<Scene, G2Map_EnterMap, Map2G_EnterMap>
	{
		protected override async ETTask Run(Scene scene, G2Map_EnterMap request, Map2G_EnterMap response)
		{
			// 这里可以从DB中加载Unit
			Unit unit = UnitFactory.Create(scene, request.PlayerId, request.UnitConfigId);
			unit.AddComponent<UnitGateInfoComponent>().ActorId = request.GateActorId;

			EntityRef<Unit> unitRef = unit;
			await scene.Fiber().WaitFrameFinish();

			unit = unitRef;
			await TransferHelper.TransferLock(unit, request.MapName, request.MapId, true);
		}
	}
}
