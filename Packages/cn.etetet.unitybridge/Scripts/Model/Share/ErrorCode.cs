namespace ET
{
    public static class UnityBridgeErrorCode
    {
        public const int Success = 0;
        public const int InvalidCommandLine = 200000000 + PackageType.UnityBridge * 1000 + 1;
        public const int Timeout = 200000000 + PackageType.UnityBridge * 1000 + 2;
        public const int NotInPlayMode = 200000000 + PackageType.UnityBridge * 1000 + 3;
        public const int AlreadyInPlayMode = 200000000 + PackageType.UnityBridge * 1000 + 4;
        public const int Compiling = 200000000 + PackageType.UnityBridge * 1000 + 5;
        public const int HandlerFail = 100000000 + PackageType.UnityBridge * 1000 + 1;
    }
}
