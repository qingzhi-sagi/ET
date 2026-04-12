using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 主切换空闲 Agent 恢复测试：
    /// 验证仅有 Agent 注册、无任何 Proxy 服务记录时，备机接管后 Agent 仍能通过Mongo主记录切换主机。
    /// </summary>
    public class Servicediscovery_FailoverIdleAgentNotification_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady(context.Fiber);
            if (addressError != 0)
            {
                Log.Console($"idle agent failover ensure address singleton failed: {addressError}");
                return 701;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_FailoverIdleAgentNotification_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"idle agent failover reset storage failed: {resetError}");
                return 700;
            }

            Fiber nodeA = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_IdleAgent_A");
            Fiber nodeB = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 1,
                "ServiceDiscovery_IdleAgent_B");
            ServiceDiscovery sdA = nodeA.Root.GetComponent<ServiceDiscovery>();
            ServiceDiscovery sdB = nodeB.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timerB = nodeB.Root.TimerComponent;
            int leasePrepareError = await ServiceDiscovery_HA_TestHelper.ConfigureFastLease(sdA, sdB, timerB);
            if (leasePrepareError != 0)
            {
                Log.Console($"idle agent failover fast lease prepare failed: {leasePrepareError}");
                return 704;
            }

            bool aMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdA, timerB, 5000);
            if (!aMaster)
            {
                Log.Console("idle agent failover nodeA not master before failover");
                return 705;
            }

            int ensureAgentError = await ServiceDiscovery_HA_TestHelper.EnsureServiceDiscoveryAgentFiberAsync(testFiber);
            if (ensureAgentError != 0)
            {
                Log.Console($"idle agent failover ensure service discovery agent failed: {ensureAgentError}");
                return 706;
            }

            Fiber agentFiber = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryAgentFiber(testFiber);
            ServiceDiscoveryAgent agent = agentFiber.Root.GetComponent<ServiceDiscoveryAgent>();
            ActorId agentActorId = agent.Root().GetActorId();
            if (agentActorId == default)
            {
                Log.Console("idle agent failover agent actor id is empty");
                return 708;
            }

            bool oldMasterHasAgent = await WaitForMasterHasAgent(sdA, timerB, agentActorId.Address, 6000);
            if (!oldMasterHasAgent)
            {
                Log.Console("idle agent failover old master does not have agent route");
                return 709;
            }

            bool agentOnOldMaster = await WaitForAgentBoundToMaster(agent, nodeA.Root.GetActorId(), timerB, 6000);
            if (!agentOnOldMaster)
            {
                Log.Console("idle agent failover agent not bound to old master before failover");
                return 710;
            }

            await testFiber.RemoveFiber(nodeA.Id);
            bool bTakeover = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdB, timerB, 10000);
            if (!bTakeover)
            {
                Log.Console("idle agent failover nodeB takeover timeout");
                return 711;
            }

            bool newMasterHasAgent = await WaitForMasterHasAgent(sdB, timerB, agentActorId.Address, 6000);
            if (!newMasterHasAgent)
            {
                Log.Console("idle agent failover new master does not have agent route");
                return 712;
            }

            bool agentSwitchedMaster = await WaitForAgentBoundToMaster(agent, nodeB.Root.GetActorId(), timerB, 6000);
            if (!agentSwitchedMaster)
            {
                Log.Console("idle agent failover agent did not switch to new master");
                return 713;
            }

            await testFiber.RemoveFiber(nodeB.Id);

            Log.Console("ServiceDiscovery IdleAgentFailoverRecovery passed");
            return ErrorCode.ERR_Success;
        }

        private static async ETTask<bool> WaitForMasterHasAgent(ServiceDiscovery master, TimerComponent timer, Address address,
            long timeoutMs)
        {
            if (master == null || timer == null || address == default)
            {
                return false;
            }

            EntityRef<ServiceDiscovery> masterRef = master;
            EntityRef<TimerComponent> timerRef = timer;
            long deadline = timer.GetSingleton<TimeInfo>().ServerNow() + timeoutMs;
            while (timer.GetSingleton<TimeInfo>().ServerNow() <= deadline)
            {
                master = masterRef;
                if (master == null)
                {
                    return false;
                }

                if (master.AgentActorIds.ContainsKey(address))
                {
                    return true;
                }

                timer = timerRef;
                if (timer == null)
                {
                    return false;
                }

                await timer.WaitAsync(50);
            }

            return false;
        }

        private static async ETTask<bool> WaitForAgentBoundToMaster(ServiceDiscoveryAgent agent, ActorId masterActorId,
            TimerComponent timer, long timeoutMs)
        {
            if (agent == null || timer == null || masterActorId == default)
            {
                return false;
            }

            EntityRef<ServiceDiscoveryAgent> agentRef = agent;
            EntityRef<TimerComponent> timerRef = timer;
            long deadline = timer.GetSingleton<TimeInfo>().ServerNow() + timeoutMs;
            while (timer.GetSingleton<TimeInfo>().ServerNow() <= deadline)
            {
                agent = agentRef;
                if (agent == null)
                {
                    return false;
                }

                if (agent.ServiceDiscoveryActorId == masterActorId &&
                    (agent.Status & ServiceDiscoveryAgentStatus.AgentRegistered) != 0)
                {
                    return true;
                }

                timer = timerRef;
                if (timer == null)
                {
                    return false;
                }

                await timer.WaitAsync(50);
            }

            return false;
        }
    }
}
