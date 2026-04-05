using System;

namespace ET.Server
{
    [EntitySystemOf(typeof(ServiceDiscoveryProxyHeartbeat))]
    public static partial class ServiceDiscoveryProxyHeartbeatSystem
    {
        [EntitySystem]
        private static void Awake(this ServiceDiscoveryProxyHeartbeat self)
        {
            self.StartTimer();
        }

        [EntitySystem]
        private static void Destroy(this ServiceDiscoveryProxyHeartbeat self)
        {
            Scene root = self.Root();
            if (root != null)
            {
                root.TimerComponent.Remove(ref self.Timer);
            }
        }

        [Invoke(TimerInvokeType.ServiceDiscoveryProxyHeartbeat)]
        public class ServiceDiscoveryProxyHeartbeatTimer : ATimer<ServiceDiscoveryProxyHeartbeat>
        {
            protected override void Run(ServiceDiscoveryProxyHeartbeat self)
            {
                self.SendHeartbeatToAgentAsync().Coroutine();
            }
        }

        private static void StartTimer(this ServiceDiscoveryProxyHeartbeat self)
        {
            if (self.Timer != 0)
            {
                return;
            }

            self.Timer = self.Root().TimerComponent.NewRepeatedTimer(
                self.Interval, TimerInvokeType.ServiceDiscoveryProxyHeartbeat, self);
        }

        private static async ETTask SendHeartbeatToAgentAsync(this ServiceDiscoveryProxyHeartbeat self)
        {
            ServiceDiscoveryProxy proxy = self.GetParent<ServiceDiscoveryProxy>();
            if (proxy == null || proxy.AgentFiberInstanceId == default || self.Sending)
            {
                return;
            }

            long now = ServiceDiscoveryHelper.GetMonotonicNow(self.Root(), "ServiceDiscoveryProxy heartbeat",
                ref self.LastRawNow, ref self.MonotonicNow);
            if (self.CircuitOpenUntil > now || self.NextRetryTime > now)
            {
                return;
            }

            self.Sending = true;
            EntityRef<ServiceDiscoveryProxyHeartbeat> selfRef = self;
            EntityRef<ServiceDiscoveryProxy> proxyRef = proxy;
            bool success = false;
            try
            {
                ServiceHeartbeatRequest request = ServiceHeartbeatRequest.Create();
                request.SceneName = proxy.Root().Name;
                using ServiceHeartbeatResponse _ =
                    await proxy.ForwardToDiscoveryByFailover<ServiceHeartbeatResponse>(request, nameof(ServiceHeartbeatRequest));
                success = true;
            }
            catch (RpcException rpcException)
            {
                proxy = proxyRef;
                if (proxy != null)
                {
                    Log.Warning(
                        $"ServiceDiscovery proxy heartbeat failed scene: {proxy.Root().Name} target: {proxy.AgentFiberInstanceId} error: {rpcException.Error} message: {rpcException.Message}");
                }
            }
            catch (Exception e)
            {
                proxy = proxyRef;
                if (proxy != null)
                {
                    Log.Warning(
                        $"ServiceDiscovery proxy heartbeat failed scene: {proxy.Root().Name} target: {proxy.AgentFiberInstanceId} error: {e.Message}");
                }
            }
            finally
            {
                self = selfRef;
                if (self != null)
                {
                    self.Sending = false;
                    self.OnRpcCompleted(success, "proxy-agent-heartbeat");
                }
            }
        }

        private static void OnRpcCompleted(this ServiceDiscoveryProxyHeartbeat self, bool success, string traceName)
        {
            if (success)
            {
                ServiceDiscoveryHelper.ResetRetryState(ref self.FailureCount, ref self.CircuitOpenUntil, ref self.NextRetryTime);
                return;
            }

            long now = ServiceDiscoveryHelper.GetMonotonicNow(self.Root(), "ServiceDiscoveryProxy heartbeat",
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
    }
}
