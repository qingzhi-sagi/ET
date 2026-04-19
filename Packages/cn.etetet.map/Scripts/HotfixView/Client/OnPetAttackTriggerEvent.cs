namespace ET.Client
{
    [Event(SceneType.Current)]
    public class OnPetAttackTriggerEvent : AEvent<Scene, OnPetAttackTrigger>
    {
        protected override async ETTask Run(Scene scene, OnPetAttackTrigger args)
        {
            Unit unit = args.Unit;
            if (unit == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            TargetComponent targetComponent = unit.GetComponent<TargetComponent>();
            if (targetComponent == null || targetComponent.Unit == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            C2M_PetAttack c2MPetAttack = C2M_PetAttack.Create();
            c2MPetAttack.UnitId = targetComponent.Unit.Id;
            unit.Root().GetComponent<ClientSenderComponent>().Send(c2MPetAttack);
            await ETTask.CompletedTask;
        }
    }
}
