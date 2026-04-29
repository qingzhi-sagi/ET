namespace ET.Server
{
    // 进入视野通知
    [Event(SceneType.Map)]
    public class UnitEnterSightRange_NotifyClient: AEvent<Scene, UnitEnterSightRange>
    {
        protected override async ETTask Run(Scene scene, UnitEnterSightRange args)
        {
            Unit a = args.A;
            Unit b = args.B;
            if (a.Id == b.Id)
            {
                return;
            }

            if (a.UnitType != UnitType.Player)
            {
                return;
            }
            
            M2C_CreateUnits createUnits = M2C_CreateUnits.Create();
            createUnits.Units.Add(UnitHelper.CreateUnitInfo(b));
            MapMessageHelper.NoticeClient(a, createUnits, NoticeType.Self);
            
            await ETTask.CompletedTask;
        }
    }
}