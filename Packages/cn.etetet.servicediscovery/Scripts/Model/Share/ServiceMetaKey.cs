namespace ET
{
    public static partial class ServiceMetaKey
    {
        public const string SceneType = nameof(SceneType);
        public const string Zone = nameof(Zone);
        public const string ActorId = nameof(ActorId);
        public const string PriorityId = nameof(PriorityId);
        public const string InnerIPOuterPort = nameof(InnerIPOuterPort);
        public const string OuterIPOuterPort = nameof(OuterIPOuterPort);
    }

    public static class ServiceDiscoveryPersistenceConst
    {
        public const string DBName = "Share";

        public const long MasterRecordId = 1;

        public const string MasterCollection = "ServiceDiscoveryMaster";

    }
}
