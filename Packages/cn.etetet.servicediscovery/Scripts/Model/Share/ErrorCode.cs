namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_ComponentNotFound = ErrorCode.ERR_WithoutException + PackageType.ServiceDiscovery * 1000 + 1; // 200050001
        public const int ERR_ServiceNotFound = ErrorCode.ERR_WithoutException + PackageType.ServiceDiscovery * 1000 + 2; // 200050002
        public const int ERR_ServiceAlreadyExists = ErrorCode.ERR_WithoutException + PackageType.ServiceDiscovery * 1000 + 3; // 200050003
        public const int ERR_SubscriberActorIdRequired = ErrorCode.ERR_WithoutException + PackageType.ServiceDiscovery * 1000 + 4; // 200050004
        public const int ERR_ServiceDiscoveryOperationFailed = ErrorCode.ERR_WithoutException + PackageType.ServiceDiscovery * 1000 + 5; // 200050005
        public const int ERR_ServiceDiscoveryFollowerRejected = ErrorCode.ERR_WithoutException + PackageType.ServiceDiscovery * 1000 + 6; // 200050006
        public const int ERR_ServiceDiscoveryInvalidArgument = ErrorCode.ERR_WithoutException + PackageType.ServiceDiscovery * 1000 + 7; // 200050007
        public const int ERR_ServiceDiscoveryPersistenceFailed = ErrorCode.ERR_WithoutException + PackageType.ServiceDiscovery * 1000 + 8; // 200050008
        public const int ERR_ServiceDiscoveryMasterUnavailable = ErrorCode.ERR_WithoutException + PackageType.ServiceDiscovery * 1000 + 9; // 200050009
    }
}
