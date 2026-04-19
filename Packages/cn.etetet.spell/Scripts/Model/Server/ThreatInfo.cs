namespace ET.Server
{
    [ChildOf(typeof(ThreatComponent))]
    public class ThreatInfo: Entity, IAwake
    {
        public EntityRef<Unit> Unit { get; set; }
        public int Threat { get; set; }
    }
}
