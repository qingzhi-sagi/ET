namespace ET.Server
{
    [Event(SceneType.Map)]
    public class AfterServerUnitCreate_AddAIBuff : AEvent<Scene, AfterServerUnitCreate>
    {
        [EnableGetComponent(typeof(NumericComponent))]
        protected override async ETTask Run(Scene scene, AfterServerUnitCreate args)
        {
            Unit unit = args.Unit;
            if (unit == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            int ai = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.AI);
            if (ai != 0)
            {
                BuffHelper.CreateBuff(unit, unit.Id, IdGenerater.Instance.GenerateId(), ai, null);
            }

            await ETTask.CompletedTask;
        }
    }
}
