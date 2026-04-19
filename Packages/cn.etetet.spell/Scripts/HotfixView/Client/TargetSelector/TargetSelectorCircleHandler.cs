namespace ET.Client
{
    public class TargetSelectorCircleHandler : TargetSelectHandler<TargetSelectorCircle>
    {
        protected override async ETTask<int> Run(TargetSelectorCircle node, EntityRef<Unit> unitRef, SpellConfig spellConfig)
        {
            Unit unit = unitRef;
            SpellIndicatorComponent spellIndicatorComponent = unit.GetComponent<SpellIndicatorComponent>();
            TargetComponent targetComponent = unit.GetComponent<TargetComponent>();
            targetComponent.Position = await spellIndicatorComponent.WaitSpellIndicator(node);
            return 0;
        }
    }
}
