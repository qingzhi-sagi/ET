namespace ET.Server
{
    public static class ServiceDiscoveryStateComponentAccessor
    {
        public static ServiceDiscoveryAgentHeartbeatComponent GetOrAddAgentHeartbeat(this ServiceDiscovery self)
        {
            ServiceDiscoveryAgentHeartbeatComponent component = self.GetComponent<ServiceDiscoveryAgentHeartbeatComponent>();
            return component ?? self.AddComponent<ServiceDiscoveryAgentHeartbeatComponent>();
        }

        public static ServiceDiscoveryLeaseComponent GetOrAddLease(this ServiceDiscovery self)
        {
            ServiceDiscoveryLeaseComponent component = self.GetComponent<ServiceDiscoveryLeaseComponent>();
            return component ?? self.AddComponent<ServiceDiscoveryLeaseComponent>();
        }

        public static ServiceDiscoveryNotificationBufferComponent GetOrAddNotificationBuffer(this ServiceDiscovery self)
        {
            ServiceDiscoveryNotificationBufferComponent component = self.GetComponent<ServiceDiscoveryNotificationBufferComponent>();
            return component ?? self.AddComponent<ServiceDiscoveryNotificationBufferComponent>();
        }

        public static ServiceDiscoveryRuntimeStateComponent GetOrAddRuntimeState(this ServiceDiscovery self)
        {
            ServiceDiscoveryRuntimeStateComponent component = self.GetComponent<ServiceDiscoveryRuntimeStateComponent>();
            return component ?? self.AddComponent<ServiceDiscoveryRuntimeStateComponent>();
        }
    }
}
