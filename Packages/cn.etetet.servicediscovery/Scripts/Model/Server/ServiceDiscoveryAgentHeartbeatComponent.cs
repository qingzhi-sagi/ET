using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(ServiceDiscovery))]
    public class ServiceDiscoveryAgentHeartbeatComponent : Entity, IAwake
    {
#if !UNITY_EDITOR
        public long HeartbeatTimeout = 10 * 1000;
#else
        public long HeartbeatTimeout = 10000 * 1000;
#endif

        public long HeartbeatCheckInterval = 1 * 1000;

        public Dictionary<Address, long> AgentHeartbeatTimes = new();
    }
}
