namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_LocationAlreadyExists = ErrorCode.ERR_WithoutException + PackageType.ActorLocation * 1000 + 7; // 200003007
        public const int ERR_LocationAlreadyLocked = ErrorCode.ERR_WithoutException + PackageType.ActorLocation * 1000 + 1; // 200003001
        public const int ERR_LocationLockNotFound = ErrorCode.ERR_WithoutException + PackageType.ActorLocation * 1000 + 2; // 200003002
        public const int ERR_LocationLockOwnerMismatch = ErrorCode.ERR_WithoutException + PackageType.ActorLocation * 1000 + 3; // 200003003
        public const int ERR_LocationLockTokenMismatch = ErrorCode.ERR_WithoutException + PackageType.ActorLocation * 1000 + 4; // 200003004
        public const int ERR_LocationPrimaryUnavailable = ErrorCode.ERR_WithoutException + PackageType.ActorLocation * 1000 + 5; // 200003005
        public const int ERR_LocationGetRetry = ErrorCode.ERR_WithoutException + PackageType.ActorLocation * 1000 + 6; // 200003006
        public const int ERR_LocationFollowerRejected = ErrorCode.ERR_WithoutException + PackageType.ActorLocation * 1000 + 8; // 200003008

        public const int ERR_ActorLocationSenderTimeout2 = ErrorCode.ERR_WithException + PackageType.ActorLocation * 1000 + 4; // 100003004
        public const int ERR_ActorLocationSenderTimeout3 = ErrorCode.ERR_WithException + PackageType.ActorLocation * 1000 + 5; // 100003005
        public const int ERR_ActorLocationSenderTimeout4 = ErrorCode.ERR_WithException + PackageType.ActorLocation * 1000 + 6; // 100003006
    }
}
