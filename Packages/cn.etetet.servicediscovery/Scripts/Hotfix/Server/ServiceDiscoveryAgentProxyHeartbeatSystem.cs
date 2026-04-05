using System;

namespace ET.Server
{
    [EntitySystemOf(typeof(ServiceDiscoveryAgentProxyHeartbeat))]
    public static partial class ServiceDiscoveryAgentProxyHeartbeatSystem
    {
        [EntitySystem]
        private static void Awake(this ServiceDiscoveryAgentProxyHeartbeat self)
        {
            self.StartTimer();
        }

        [EntitySystem]
        private static void Destroy(this ServiceDiscoveryAgentProxyHeartbeat self)
        {
            Scene root = self.Root();
            if (root != null)
            {
                root.TimerComponent.Remove(ref self.Timer);
            }
        }

        [Invoke(TimerInvokeType.ServiceDiscoveryAgentProxyHeartbeatCheck)]
        public class ServiceDiscoveryAgentProxyHeartbeatTimer : ATimer<ServiceDiscoveryAgentProxyHeartbeat>
        {
            protected override void Run(ServiceDiscoveryAgentProxyHeartbeat self)
            {
                self.CheckHeartbeatTimeoutAsync().Coroutine();
            }
        }

        private static void StartTimer(this ServiceDiscoveryAgentProxyHeartbeat self)
        {
            if (self.Timer != 0)
            {
                return;
            }

            self.Timer = self.Root().TimerComponent.NewRepeatedTimer(
                self.CheckInterval, TimerInvokeType.ServiceDiscoveryAgentProxyHeartbeatCheck, self);
        }

        public static void TouchProxyHeartbeat(this ServiceDiscoveryAgentProxyHeartbeat self, string sceneName, bool forceAdd = false)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            ServiceDiscoveryAgent agent = self.GetParent<ServiceDiscoveryAgent>();
            if (agent == null)
            {
                return;
            }

            if (!forceAdd && !self.HeartbeatTimes.ContainsKey(sceneName) && !self.IsLocalProxyScene(agent, sceneName))
            {
                return;
            }

            self.HeartbeatTimes[sceneName] = ServiceDiscoveryHelper.GetMonotonicNow(self.Root(),
                "ServiceDiscoveryAgent proxy-heartbeat", ref self.LastRawNow, ref self.MonotonicNow);
            self.TimeoutUnregisterFailureCounts.Remove(sceneName);
            self.TimeoutUnregisterNextRetryTimes.Remove(sceneName);
        }

        public static void RemoveProxyHeartbeat(this ServiceDiscoveryAgentProxyHeartbeat self, string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            self.HeartbeatTimes.Remove(sceneName);
            self.TimeoutUnregisterFailureCounts.Remove(sceneName);
            self.TimeoutUnregisterNextRetryTimes.Remove(sceneName);
        }

        private static async ETTask CheckHeartbeatTimeoutAsync(this ServiceDiscoveryAgentProxyHeartbeat self)
        {
            ServiceDiscoveryAgent agent = self.GetParent<ServiceDiscoveryAgent>();
            if (agent == null || self.Checking)
            {
                return;
            }

            self.Checking = true;
            EntityRef<ServiceDiscoveryAgentProxyHeartbeat> selfRef = self;
            EntityRef<ServiceDiscoveryAgent> agentRef = agent;
            try
            {
                self.EnsureTrackedLocalProxyScenes(agent);
                await self.CheckProxyHeartbeatTimeoutAsync(agent);
            }
            finally
            {
                self = selfRef;
                if (self != null)
                {
                    self.Checking = false;
                }
            }
        }

        private static void EnsureTrackedLocalProxyScenes(this ServiceDiscoveryAgentProxyHeartbeat self, ServiceDiscoveryAgent agent)
        {
            long now = ServiceDiscoveryHelper.GetMonotonicNow(self.Root(), "ServiceDiscoveryAgent proxy-heartbeat",
                ref self.LastRawNow, ref self.MonotonicNow);
            Address innerAddress = AddressSingleton.Instance.InnerAddress;
            foreach ((string sceneName, EntityRef<ServiceInfo> serviceRef) in agent.SceneNameServices)
            {
                ServiceInfo serviceInfo = serviceRef;
                if (serviceInfo == null || serviceInfo.ActorId == default)
                {
                    continue;
                }

                if (serviceInfo.ActorId.Address != innerAddress)
                {
                    continue;
                }

                if (!self.HeartbeatTimes.ContainsKey(sceneName))
                {
                    self.HeartbeatTimes[sceneName] = now;
                }
            }
        }

        private static bool IsLocalProxyScene(this ServiceDiscoveryAgentProxyHeartbeat self, ServiceDiscoveryAgent agent, string sceneName)
        {
            if (!agent.LocalPublishedServices.TryGetValue(sceneName, out (ActorId ActorId, StringKV Metadata) localService))
            {
                return false;
            }

            Address innerAddress = AddressSingleton.Instance.InnerAddress;
            return localService.ActorId != default && localService.ActorId.Address == innerAddress;
        }

        private static async ETTask CheckProxyHeartbeatTimeoutAsync(this ServiceDiscoveryAgentProxyHeartbeat self, ServiceDiscoveryAgent agent)
        {
            EntityRef<ServiceDiscoveryAgentProxyHeartbeat> selfRef = self;
            EntityRef<ServiceDiscoveryAgent> agentRef = agent;
            long now = ServiceDiscoveryHelper.GetMonotonicNow(self.Root(), "ServiceDiscoveryAgent proxy-heartbeat",
                ref self.LastRawNow, ref self.MonotonicNow);
            using ListComponent<string> staleScenes = ListComponent<string>.Create();
            using ListComponent<string> timeoutScenes = ListComponent<string>.Create();
            foreach ((string sceneName, long lastHeartbeatTime) in self.HeartbeatTimes)
            {
                if (!self.IsLocalProxyScene(agent, sceneName))
                {
                    staleScenes.Add(sceneName);
                    continue;
                }

                if (now - lastHeartbeatTime > self.Timeout && self.CanRetryTimeoutUnregister(sceneName, now))
                {
                    timeoutScenes.Add(sceneName);
                }
            }

            foreach (string sceneName in staleScenes)
            {
                self.RemoveProxyHeartbeat(sceneName);
                agent.UnsubscribeProxyServiceChange(sceneName);
            }

            foreach (string sceneName in timeoutScenes)
            {
                self = selfRef;
                agent = agentRef;
                if (self == null || agent == null)
                {
                    return;
                }

                if (!self.IsLocalProxyScene(agent, sceneName))
                {
                    self.RemoveProxyHeartbeat(sceneName);
                    continue;
                }

                try
                {
                    agent.RemoveLocalServiceAfterUnregister(sceneName);
                    agent.UnsubscribeProxyServiceChange(sceneName);
                    self.RemoveProxyHeartbeat(sceneName);
                    self.OnTimeoutUnregisterCompleted(sceneName, true, ServiceDiscoveryHelper.GetMonotonicNow(self.Root(),
                        "ServiceDiscoveryAgent proxy-heartbeat", ref self.LastRawNow, ref self.MonotonicNow));
                    Log.Warning(
                        $"ServiceDiscoveryAgent proxy heartbeat timeout, unregister scene: {sceneName} agent: {agent.Root().Name}");

                    await ETTask.CompletedTask;
                }
                catch (Exception e)
                {
                    self = selfRef;
                    if (self != null)
                    {
                        self.OnTimeoutUnregisterCompleted(sceneName, false, ServiceDiscoveryHelper.GetMonotonicNow(self.Root(),
                            "ServiceDiscoveryAgent proxy-heartbeat", ref self.LastRawNow, ref self.MonotonicNow));
                    }

                    agent = agentRef;
                    if (agent != null)
                    {
                        Log.Warning(
                            $"ServiceDiscoveryAgent proxy timeout unregister failed scene: {sceneName} agent: {agent.Root().Name} error: {e.Message}");
                    }
                }
            }
        }

        private static bool CanRetryTimeoutUnregister(this ServiceDiscoveryAgentProxyHeartbeat self, string sceneName, long now)
        {
            return !self.TimeoutUnregisterNextRetryTimes.TryGetValue(sceneName, out long nextRetryTime) || now >= nextRetryTime;
        }

        private static void OnTimeoutUnregisterCompleted(this ServiceDiscoveryAgentProxyHeartbeat self, string sceneName, bool success, long now)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            if (success)
            {
                self.TimeoutUnregisterFailureCounts.Remove(sceneName);
                self.TimeoutUnregisterNextRetryTimes.Remove(sceneName);
                return;
            }

            int failureCount = 1;
            if (self.TimeoutUnregisterFailureCounts.TryGetValue(sceneName, out int count))
            {
                failureCount = count + 1;
            }

            self.TimeoutUnregisterFailureCounts[sceneName] = failureCount;
            long backoff = ServiceDiscoveryHelper.GetRetryBackoff(failureCount, self.TimeoutUnregisterBackoffBase,
                self.TimeoutUnregisterBackoffMax);
            self.TimeoutUnregisterNextRetryTimes[sceneName] = now + backoff;
        }

    }
}
