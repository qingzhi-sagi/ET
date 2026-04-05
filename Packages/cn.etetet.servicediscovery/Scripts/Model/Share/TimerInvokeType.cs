namespace ET
{
    public static partial class TimerInvokeType
    {
        public const int ServiceDiscoveryProxyHeartbeat = PackageType.ServiceDiscovery * 1000 + 2;
        public const int ServiceDiscoveryAgentProxyHeartbeatCheck = PackageType.ServiceDiscovery * 1000 + 3;
    }
}
