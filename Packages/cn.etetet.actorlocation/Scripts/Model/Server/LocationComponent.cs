using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET.Server
{
    public static class LocationPersistenceConst
    {
        public const string DBName = "Share";

        public const string RouteCollection = "LocationInfo";
    }

    public struct LocationTypeState
    {
        public ActorId ActorId;

        public long LockToken;

        public long LockExpireTime;
    }

    [ChildOf(typeof(LocationComponent))]
    public class LocationInfo: Entity, IAwake, IDestroy
    {
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, LocationTypeState> TypeStates = new();
    }

    [ComponentOf(typeof(Scene))]
    public class LocationComponent: Entity, IAwake
    {
        public bool IsPrimaryLocation;

        public string PrimaryLocationSceneName;
    }
}
