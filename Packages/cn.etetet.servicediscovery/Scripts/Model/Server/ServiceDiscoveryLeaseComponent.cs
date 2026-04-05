namespace ET.Server
{
    [ComponentOf(typeof(ServiceDiscovery))]
    public class ServiceDiscoveryLeaseComponent : Entity, IAwake
    {
        public string CurrentMasterSceneName;

        public ActorId CurrentMasterActorId;

        public long CurrentMasterEpoch;

        public long CurrentMasterLeaseExpireTime;

        public bool IsActiveMaster;

#if !UNITY_EDITOR
        public long MasterLeaseTimeout = 5 * 1000;
#else
        public long MasterLeaseTimeout = 30 * 1000;
#endif

        public long MasterLeaseRenewInterval = 1 * 1000;

        public long LastMasterLeaseCheckTime;

        public bool LeaseTickRunning;

        public int LeaseFailureCount;

        public long LeaseCircuitOpenUntil;

        public long NextLeaseRetryTime;

        public int LeaseCircuitThreshold = 5;

        public long LeaseCircuitOpenDuration = 3 * 1000;

        public long LeaseFailureBackoffBase = 100;

        public long LeaseFailureBackoffMax = 2 * 1000;
    }
}
