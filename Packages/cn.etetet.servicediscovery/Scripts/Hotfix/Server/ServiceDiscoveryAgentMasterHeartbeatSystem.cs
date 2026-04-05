using System;

namespace ET.Server
{
    [EntitySystemOf(typeof(ServiceDiscoveryAgentMasterHeartbeat))]
    public static partial class ServiceDiscoveryAgentMasterHeartbeatSystem
    {
        [EntitySystem]
        private static void Awake(this ServiceDiscoveryAgentMasterHeartbeat self)
        {
            self.HeartbeatLoopAsync().Coroutine();
        }

        [EntitySystem]
        private static void Destroy(this ServiceDiscoveryAgentMasterHeartbeat self)
        {
        }

        private static async ETTask HeartbeatLoopAsync(this ServiceDiscoveryAgentMasterHeartbeat self)
        {
            EntityRef<ServiceDiscoveryAgentMasterHeartbeat> selfRef = self;
            ServiceDiscoveryAgent agent = self.GetParent<ServiceDiscoveryAgent>();
            EntityRef<ServiceDiscoveryAgent> agentRef = agent;

            while (true)
            {
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                long interval = self.Interval > 0 ? self.Interval : 1;
                await self.Root().TimerComponent.WaitAsync(interval);

                self = selfRef;
                agent = agentRef;
                if (self == null || agent == null)
                {
                    return;
                }

                await self.SendHeartbeatToDiscoveryAsync();
            }
        }

        private static async ETTask SendHeartbeatToDiscoveryAsync(this ServiceDiscoveryAgentMasterHeartbeat self)
        {
            ServiceDiscoveryAgent agent = self.GetParent<ServiceDiscoveryAgent>();
            if (agent == null)
            {
                return;
            }

            long now = ServiceDiscoveryHelper.GetMonotonicNow(self.Root(), "ServiceDiscoveryAgent heartbeat",
                ref self.LastRawNow, ref self.MonotonicNow);
            if (self.CircuitOpenUntil > now || self.NextRetryTime > now)
            {
                return;
            }

            EntityRef<ServiceDiscoveryAgentMasterHeartbeat> selfRef = self;
            EntityRef<ServiceDiscoveryAgent> agentRef = agent;
            bool success = false;
            bool shouldRecordResult = false;
            try
            {
                self = selfRef;
                agent = agentRef;
                if (self == null || agent == null || !agent.IsReady())
                {
                    return;
                }

                Scene agentRoot = agent.Root();
                ActorId agentActorId = agentRoot.GetActorId();
                ActorId targetActorId = agent.ServiceDiscoveryActorId;
                string sceneName = agentRoot.Name;
                long rpcTimeout = self.RpcTimeout;
                shouldRecordResult = true;
                try
                {
                    using ServiceHeartbeatRequest request = ServiceHeartbeatRequest.Create();
                    request.AgentActorId = agentActorId;
                    using ServiceHeartbeatResponse _ = await agent.MessageSender.Call(targetActorId, request, rpcTimeout)
                        as ServiceHeartbeatResponse;
                    success = true;
                }
                catch (Exception e)
                {
                    agent = agentRef;
                    if (agent != null)
                    {
                        if (ShouldMarkMasterUnavailable(e))
                        {
                            agent.InvalidateEndpointAndTriggerBackgroundRegister("heartbeat-failure");
                        }

                        Log.Warning(
                            $"ServiceDiscoveryAgent heartbeat failed agent: {agentActorId} scene: {sceneName} error: {e.Message}");
                    }
                }
            }
            finally
            {
                self = selfRef;
                if (self != null)
                {
                    if (shouldRecordResult)
                    {
                        self.OnRpcCompleted(success, "agent-master-heartbeat");
                    }
                }
            }
        }

        private static void OnRpcCompleted(this ServiceDiscoveryAgentMasterHeartbeat self, bool success, string traceName)
        {
            if (success)
            {
                ServiceDiscoveryHelper.ResetRetryState(ref self.FailureCount, ref self.CircuitOpenUntil, ref self.NextRetryTime);
                return;
            }

            long now = ServiceDiscoveryHelper.GetMonotonicNow(self.Root(), "ServiceDiscoveryAgent heartbeat",
                ref self.LastRawNow, ref self.MonotonicNow);
            bool circuitOpened = ServiceDiscoveryHelper.RecordRetryFailure(ref self.FailureCount, ref self.CircuitOpenUntil,
                ref self.NextRetryTime, self.CircuitThreshold, self.CircuitOpenDuration, self.RetryBackoffBase,
                self.RetryBackoffMax, now);
            if (circuitOpened)
            {
                Log.Warning(
                    $"ServiceDiscovery heartbeat circuit opened trace: {traceName} failureCount: {self.FailureCount} openUntil: {self.CircuitOpenUntil}");
            }
        }

        private static bool ShouldMarkMasterUnavailable(Exception e)
        {
            if (e is not RpcException rpcException)
            {
                return true;
            }

            return rpcException.Error == ErrorCode.ERR_Cancel
                   || rpcException.Error == ErrorCode.ERR_Timeout
                   || ServiceDiscoveryErrorHelper.ShouldTriggerFailover(rpcException.Error);
        }
    }
}
