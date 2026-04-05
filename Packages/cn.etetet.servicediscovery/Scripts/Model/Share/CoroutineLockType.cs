namespace ET
{
    public static partial class CoroutineLockType
    {
        public const int ServiceDiscoveryAgentInit = PackageType.ServiceDiscovery * 1000 + 5; // ServiceDiscoveryAgent显式初始化
        public const int ServiceDiscoveryServiceMutation = PackageType.ServiceDiscovery * 1000 + 6; // ServiceDiscovery服务注册/注销串行化
        public const int ServiceDiscoveryMasterLease = PackageType.ServiceDiscovery * 1000 + 9; // ServiceDiscovery主租约状态切换串行化
    }
}
