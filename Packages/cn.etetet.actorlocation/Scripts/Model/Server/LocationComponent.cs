namespace ET.Server
{
    public static class LocationPersistenceConst
    {
        public const string DBName = "Share";

        public const string RouteCollection = "LocationInfo";
    }

    public struct LocationState
    {
        public bool Exists;
        public ActorId ActorId;
        public long LockToken;
        public long LockExpireTime;
    }

    [ChildOf(typeof(LocationOneType))]
    public class LocationInfo: Entity, IAwake<ActorId>, IDestroy
    {
        public ActorId ActorId;

        public long LockToken;

        public long LockExpireTime;
    }

    [ChildOf(typeof(LocationManagerComponent))]
    public class LocationOneType: Entity, IAwake
    {
    }

    [ComponentOf(typeof(Scene))]
    public class LocationManagerComponent: Entity, IAwake
    {
        public bool IsPrimaryLocation;

        public string PrimaryLocationSceneName;
    }
}
