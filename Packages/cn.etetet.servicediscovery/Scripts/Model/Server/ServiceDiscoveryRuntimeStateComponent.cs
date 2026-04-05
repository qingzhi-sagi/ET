namespace ET.Server
{
    [ComponentOf(typeof(ServiceDiscovery))]
    public class ServiceDiscoveryRuntimeStateComponent : Entity, IAwake
    {
        public long LastRawNow;

        public long MonotonicNow;
    }
}
