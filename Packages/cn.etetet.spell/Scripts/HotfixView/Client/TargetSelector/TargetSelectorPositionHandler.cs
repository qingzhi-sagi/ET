namespace ET.Client
{
    public class TargetSelectorPositionHandler: TargetSelectHandler<TargetSelectorPosition>
    {
        protected override async ETTask<int> Run(TargetSelectorPosition node, EntityRef<Unit> unitRef, SpellConfig spellConfig)
        {
            Unit unit = unitRef;
            SpellIndicatorComponent spellIndicatorComponent = unit.GetComponent<SpellIndicatorComponent>();
            TargetComponent targetComponent = unit.GetComponent<TargetComponent>();
            targetComponent.Position = await spellIndicatorComponent.WaitSpellIndicator(node);
            return 0;
        }
    }
}
