namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class ServiceDiscoveryTestSlowHeartbeatEndpoint : Entity, IAwake
    {
        public long HeartbeatDelayMs = 150;

        public int RegisterRequestCount;

        public int HeartbeatRequestCount;

        public int CurrentHeartbeatRequests;

        public int MaxConcurrentHeartbeatRequests;
    }
}
