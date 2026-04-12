using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    public class Servicediscovery_StaleNotificationFencing_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady();
            if (addressError != 0)
            {
                Log.Console($"stale notification ensure address singleton failed: {addressError}");
                return 2;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_StaleNotificationFencing_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"stale notification reset storage failed: {resetError}");
                return 1;
            }

            Fiber nodeA = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_StaleNotification_A");
            Fiber nodeB = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 1,
                "ServiceDiscovery_StaleNotification_B");
            ServiceDiscovery sdA = nodeA.Root.GetComponent<ServiceDiscovery>();
            ServiceDiscovery sdB = nodeB.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timerB = nodeB.Root.TimerComponent;
            int leasePrepareError = await ServiceDiscovery_HA_TestHelper.ConfigureFastLease(sdA, sdB, timerB);
            if (leasePrepareError != 0)
            {
                Log.Console($"stale notification fast lease prepare failed: {leasePrepareError}");
                return 5;
            }

            bool aMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdA, timerB, 5000);
            if (!aMaster)
            {
                Log.Console("stale notification nodeA not master before failover");
                return 6;
            }

            (int beforeError, string beforeSceneName, ActorId beforeMasterActorId, long beforeEpoch, _) =
                await ServiceDiscovery_HA_TestHelper.QueryMasterRecord(sdA);
            if (beforeError != 0 || beforeMasterActorId == default || beforeEpoch <= 0)
            {
                Log.Console($"stale notification query old master record failed: {beforeError}");
                return 7;
            }

            Fiber watcherFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "Watcher_StaleNotification",
                new StringKV { { "Role", "Watcher" } });
            if (watcherFiber == null)
            {
                Log.Console("stale notification create watcher fiber failed");
                return 8;
            }

            ServiceDiscoveryProxy watcherProxy = watcherFiber.Root.GetComponent<ServiceDiscoveryProxy>();
            MessageSender watcherSender = watcherFiber.Root.GetComponent<MessageSender>();
            TimerComponent watcherTimer = watcherFiber.Root.TimerComponent;
            await watcherProxy.SubscribeServiceChange("InjectedRole", new StringKV { { "Role", "Injected" } });

            Fiber agentFiber = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryAgentFiber(testFiber);
            ServiceDiscoveryAgent agent = agentFiber.Root.GetComponent<ServiceDiscoveryAgent>();
            bool agentOnOldMaster = await WaitForAgentBoundToMasterAndEpoch(agent, beforeMasterActorId, beforeEpoch, timerB, 6000);
            if (!agentOnOldMaster)
            {
                Log.Console("stale notification agent not bound to old master before failover");
                return 11;
            }

            await testFiber.RemoveFiber(nodeA.Id);

            bool bTakeover = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdB, timerB, 10000);
            if (!bTakeover)
            {
                Log.Console("stale notification nodeB takeover timeout");
                return 12;
            }

            (int afterError, string afterSceneName, ActorId afterMasterActorId, long afterEpoch, _) =
                await ServiceDiscovery_HA_TestHelper.QueryMasterRecord(sdB);
            if (afterError != 0 || afterMasterActorId == default || afterEpoch <= beforeEpoch)
            {
                Log.Console($"stale notification query new master record failed: {afterError}");
                return 13;
            }

            bool agentOnNewMaster = await WaitForAgentBoundToMasterAndEpoch(agent, afterMasterActorId, afterEpoch, timerB, 6000);
            if (!agentOnNewMaster)
            {
                Log.Console("stale notification agent not bound to new master after failover");
                return 14;
            }

            string wrongSourceSceneName = "Injected_WrongSource";
            watcherSender.Send(agent.Root().GetActorId(), CreateNotification(1, afterEpoch, beforeMasterActorId, wrongSourceSceneName,
                afterMasterActorId, new StringKV { { "Role", "Injected" } }));

            bool wrongSourceInAgent = await WaitForAgentHasService(agent, timerB, wrongSourceSceneName, 1000);
            if (wrongSourceInAgent)
            {
                Log.Console("stale notification current epoch but wrong source actor was accepted by agent");
                return 15;
            }

            bool wrongSourceInProxy = await ServiceDiscovery_HA_TestHelper.WaitForProxyHasService(watcherProxy, watcherTimer,
                wrongSourceSceneName, 1000);
            if (wrongSourceInProxy)
            {
                Log.Console("stale notification current epoch but wrong source actor polluted proxy cache");
                return 16;
            }

            string staleEpochSceneName = "Injected_StaleEpoch";
            watcherSender.Send(agent.Root().GetActorId(), CreateNotification(1, beforeEpoch, beforeMasterActorId, staleEpochSceneName,
                afterMasterActorId, new StringKV { { "Role", "Injected" } }));

            bool staleEpochInAgent = await WaitForAgentHasService(agent, timerB, staleEpochSceneName, 1000);
            if (staleEpochInAgent)
            {
                Log.Console("stale notification old epoch was accepted by agent");
                return 17;
            }

            bool staleEpochInProxy = await ServiceDiscovery_HA_TestHelper.WaitForProxyHasService(watcherProxy, watcherTimer,
                staleEpochSceneName, 1000);
            if (staleEpochInProxy)
            {
                Log.Console("stale notification old epoch polluted proxy cache");
                return 18;
            }

            string currentSceneName = "Injected_CurrentEpoch";
            watcherSender.Send(agent.Root().GetActorId(), CreateNotification(1, afterEpoch, afterMasterActorId, currentSceneName,
                afterMasterActorId, new StringKV { { "Role", "Injected" } }));

            bool currentInAgent = await WaitForAgentHasService(agent, timerB, currentSceneName, 2000);
            if (!currentInAgent)
            {
                Log.Console("stale notification current epoch notification was not accepted by agent");
                return 19;
            }

            bool currentInProxy = await ServiceDiscovery_HA_TestHelper.WaitForProxyHasService(watcherProxy, watcherTimer,
                currentSceneName, 2000);
            if (!currentInProxy)
            {
                Log.Console("stale notification current epoch notification did not reach proxy");
                return 20;
            }

            await testFiber.RemoveFiber(watcherFiber.Id);
            await testFiber.RemoveFiber(nodeB.Id);

            Log.Console($"ServiceDiscovery StaleNotificationFencing passed, old: {beforeSceneName}/{beforeEpoch}, new: {afterSceneName}/{afterEpoch}");
            return ErrorCode.ERR_Success;
        }

        private static ServiceChangeNotification CreateNotification(int changeType, long epoch, ActorId masterActorId,
            string sceneName, ActorId actorId, StringKV metadata)
        {
            ServiceChangeNotification notification = ServiceChangeNotification.Create();
            notification.ChangeType = changeType;
            notification.Epoch = epoch;
            notification.MasterActorId = masterActorId;

            ServiceInfoProto serviceInfo = ServiceInfoProto.Create();
            serviceInfo.SceneName = sceneName;
            serviceInfo.ActorId = actorId;
            if (metadata != null)
            {
                foreach ((string key, string value) in metadata)
                {
                    serviceInfo.Metadata[key] = value;
                }
            }

            notification.ServiceInfo.Add(serviceInfo);
            return notification;
        }

        private static async ETTask<bool> WaitForAgentBoundToMasterAndEpoch(ServiceDiscoveryAgent agent, ActorId masterActorId,
            long epoch, TimerComponent timer, long timeoutMs)
        {
            if (agent == null || masterActorId == default || epoch <= 0 || timer == null)
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
                    agent.CurrentMasterEpoch == epoch &&
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

        private static async ETTask<bool> WaitForAgentHasService(ServiceDiscoveryAgent agent, TimerComponent timer, string sceneName,
            long timeoutMs)
        {
            if (agent == null || timer == null || string.IsNullOrEmpty(sceneName))
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

                if (agent.SceneNameServices.TryGetValue(sceneName, out EntityRef<ServiceInfo> serviceRef) && serviceRef != null)
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
