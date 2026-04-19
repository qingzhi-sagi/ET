namespace ET.Client
{
    [Event(SceneType.Current)]
    public class OnSpellTriggerEvent: AEvent<Scene, OnSpellTrigger>
    {
        protected override async ETTask Run(Scene scene, OnSpellTrigger args)
        {
            int spellConfigId = args.SpellConfigId;
            SpellConfig spellConfig = scene.Fiber().GetSingleton<SpellConfigCategory>().Get(spellConfigId);
            
            C2M_SpellCast c2MSpellCast = C2M_SpellCast.Create();
            c2MSpellCast.SpellConfigId = spellConfigId;
            
            Unit unit = args.Unit;
            EntityRef<Unit> unitRef = unit;
            await TargetSelectDispatcher.Instance.Handle(spellConfig.TargetSelector, unitRef, spellConfig);

            unit = unitRef;
            TargetComponent targetComponent = unit.GetComponent<TargetComponent>();
            c2MSpellCast.TargetPosition = targetComponent.Position;
            SpellHelper.Cast(unit, c2MSpellCast);
        }
    }
}
