using System.Collections.Generic;
using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 故障恢复与客户端状态测试：
    /// 验证主挂掉后备接管，以及注册/订阅/本地缓存状态恢复完整性。
    /// </summary>
public class Servicediscovery_FailureRecoveryAndClientState_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            // 1. 准备测试环境

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady();
            if (addressError != 0)
            {
                Log.Console($"recovery ensure address singleton failed: {addressError}");
                return 511;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_FailureRecoveryAndClientState_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"recovery reset storage failed: {resetError}");
                return 500;
            }

            List<StartSceneConfig> serviceDiscoveryConfigs = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryConfigs();
            if (serviceDiscoveryConfigs.Count == 0)
            {
                Log.Console("recovery no service discovery config found");
                return 512;
            }

            // 2. 拉起主备并确认主节点可写
            Fiber nodeA = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                serviceDiscoveryConfigs[0].Name);
            Fiber nodeB = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 1,
                "ServiceDiscovery_RecoveryFollower");
            ServiceDiscovery sdA = nodeA.Root.GetComponent<ServiceDiscovery>();
            ServiceDiscovery sdB = nodeB.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timerB = nodeB.Root.TimerComponent;
            int leasePrepareError = await ServiceDiscovery_HA_TestHelper.ConfigureFastLease(sdA, sdB, timerB);
            if (leasePrepareError != 0)
            {
                Log.Console($"recovery fast lease prepare failed: {leasePrepareError}");
                return 503;
            }

            bool aMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdA, timerB, 5000);
            if (!aMaster)
            {
                Log.Console("recovery nodeA not master before failover");
                return 504;
            }

            // 3. 创建 provider/client，并建立订阅关系
            Fiber providerFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "ServiceProvider_Recovery",
                new StringKV { { "Role", "Provider" } });
            Fiber clientFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "ServiceClient_Recovery",
                new StringKV { { "Role", "Client" } });
            if (providerFiber == null || clientFiber == null)
            {
                Log.Console("recovery create provider/client fiber failed");
                return 505;
            }

            ServiceDiscoveryProxy clientProxy = clientFiber.Root.GetComponent<ServiceDiscoveryProxy>();
            TimerComponent clientTimer = clientFiber.Root.TimerComponent;
            await clientProxy.SubscribeServiceChange("ProviderRoleFilter", new StringKV { { "Role", "Provider" } });

            bool providerVisible = await ServiceDiscovery_HA_TestHelper.WaitForProxyHasService(clientProxy, clientTimer,
                providerFiber.Root.Name, 6000);
            if (!providerVisible)
            {
                Log.Console("recovery provider service not visible in client before failover");
                return 507;
            }

            // 4. 模拟主故障，验证备机接管时长
            long switchBegin = testFiber.GetSingleton<TimeInfo>().ServerNow();
            await testFiber.RemoveFiber(nodeA.Id);
            bool bTakeover = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdB, timerB, 10000);
            if (!bTakeover)
            {
                Log.Console("recovery nodeB takeover timeout");
                return 508;
            }

            long switchCost = testFiber.GetSingleton<TimeInfo>().ServerNow() - switchBegin;
            if (switchCost > 6000)
            {
                Log.Console($"recovery switch too slow, cost: {switchCost}ms");
                return 509;
            }

            // 5. 验证服务注册、订阅关系和客户端缓存均恢复
            bool providerRegistered =
                await ServiceDiscovery_HA_TestHelper.WaitForMasterHasService(sdB, timerB, providerFiber.Root.Name, 6000);
            if (!providerRegistered)
            {
                Log.Console("recovery provider not registered on new master");
                return 510;
            }

            bool clientRegistered = await ServiceDiscovery_HA_TestHelper.WaitForMasterHasService(sdB, timerB, clientFiber.Root.Name, 6000);
            if (!clientRegistered)
            {
                Log.Console("recovery client not registered on new master");
                return 513;
            }

            bool subscriberRecovered = await ServiceDiscovery_HA_TestHelper.WaitForMasterHasSubscriber(sdB, testFiber, timerB,
                clientFiber.Root.Name, "ProviderRoleFilter", 6000);
            if (!subscriberRecovered)
            {
                Log.Console("recovery client subscriber/filter not recovered on new master");
                return 514;
            }

            bool providerVisibleAfter = await ServiceDiscovery_HA_TestHelper.WaitForProxyHasService(clientProxy, clientTimer,
                providerFiber.Root.Name, 6000);
            if (!providerVisibleAfter)
            {
                Log.Console("recovery provider not visible in client after failover");
                return 515;
            }

            if (!clientProxy.SubscribeFilters.ContainsKey("ProviderRoleFilter"))
            {
                Log.Console("recovery client proxy filter cache missing");
                return 516;
            }

            // 测试成功路径显式控制释放顺序：先释放业务 proxy，再释放 service discovery。
            await testFiber.RemoveFiber(providerFiber.Id);
            await testFiber.RemoveFiber(clientFiber.Id);
            await testFiber.RemoveFiber(nodeB.Id);

            Log.Console($"ServiceDiscovery Recovery passed, switchCost: {switchCost}ms");
            return ErrorCode.ERR_Success;
        }
    }

    [SkipAwaitEntityCheck]
    /// <summary>
    /// 主切换后同名服务元数据变化回放测试：
    /// 验证 agent 全量重同步时会按旧元数据先回放 remove，再按新元数据回放 add。
    /// </summary>
    public class Servicediscovery_FailoverMetadataChangeReplay_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady();
            if (addressError != 0)
            {
                Log.Console($"metadata replay ensure address singleton failed: {addressError}");
                return 601;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_FailoverMetadataChangeReplay_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"metadata replay reset storage failed: {resetError}");
                return 600;
            }

            Fiber nodeA = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_MetadataReplay_A");
            Fiber nodeB = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 1,
                "ServiceDiscovery_MetadataReplay_B");
            ServiceDiscovery sdA = nodeA.Root.GetComponent<ServiceDiscovery>();
            ServiceDiscovery sdB = nodeB.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timerB = nodeB.Root.TimerComponent;
            int leasePrepareError = await ServiceDiscovery_HA_TestHelper.ConfigureFastLease(sdA, sdB, timerB);
            if (leasePrepareError != 0)
            {
                Log.Console($"metadata replay fast lease prepare failed: {leasePrepareError}");
                return 604;
            }

            bool aMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdA, timerB, 5000);
            if (!aMaster)
            {
                Log.Console("metadata replay nodeA not master before failover");
                return 605;
            }

            Fiber providerFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "ServiceProvider_MetadataReplay",
                new StringKV { { "Role", "Provider" } });
            Fiber oldWatcherFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "WatcherOld_MetadataReplay",
                new StringKV { { "Role", "WatcherOld" } });
            Fiber newWatcherFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "WatcherNew_MetadataReplay",
                new StringKV { { "Role", "WatcherNew" } });
            if (providerFiber == null || oldWatcherFiber == null || newWatcherFiber == null)
            {
                Log.Console("metadata replay create proxy fibers failed");
                return 606;
            }

            ServiceDiscoveryProxy oldWatcherProxy = oldWatcherFiber.Root.GetComponent<ServiceDiscoveryProxy>();
            ServiceDiscoveryProxy newWatcherProxy = newWatcherFiber.Root.GetComponent<ServiceDiscoveryProxy>();
            TimerComponent oldWatcherTimer = oldWatcherFiber.Root.TimerComponent;
            TimerComponent newWatcherTimer = newWatcherFiber.Root.TimerComponent;
            await oldWatcherProxy.SubscribeServiceChange("RoleProvider", new StringKV { { "Role", "Provider" } });
            await newWatcherProxy.SubscribeServiceChange("RoleProviderV2", new StringKV { { "Role", "ProviderV2" } });

            string providerSceneName = providerFiber.Root.Name;

            bool oldWatcherHasProvider = await ServiceDiscovery_HA_TestHelper.WaitForProxyHasService(oldWatcherProxy, oldWatcherTimer,
                providerSceneName, 6000);
            if (!oldWatcherHasProvider)
            {
                Log.Console("metadata replay old watcher did not receive initial provider");
                return 608;
            }

            bool newWatcherUnexpectedHasProvider = await ServiceDiscovery_HA_TestHelper.WaitForProxyHasService(newWatcherProxy, newWatcherTimer,
                providerSceneName, 1000);
            if (newWatcherUnexpectedHasProvider)
            {
                Log.Console("metadata replay new watcher unexpectedly received initial provider");
                return 609;
            }

            bool overrideOk = TrySetAgentLocalServiceRole(testFiber, providerSceneName, "ProviderV2");
            if (!overrideOk)
            {
                Log.Console("metadata replay override agent local service role failed");
                return 610;
            }

            await testFiber.RemoveFiber(nodeA.Id);
            bool bTakeover = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdB, timerB, 10000);
            if (!bTakeover)
            {
                Log.Console("metadata replay nodeB takeover timeout");
                return 611;
            }

            bool providerRegisteredOnB =
                await ServiceDiscovery_HA_TestHelper.WaitForMasterHasService(sdB, timerB, providerSceneName, 6000);
            if (!providerRegisteredOnB)
            {
                Log.Console("metadata replay provider not registered on new master");
                return 612;
            }

            bool oldWatcherRemoved = await ServiceDiscovery_HA_TestHelper.WaitForProxyNotHasService(oldWatcherProxy, oldWatcherTimer,
                providerSceneName, 6000);
            if (!oldWatcherRemoved)
            {
                Log.Console("metadata replay old watcher did not receive remove for old role");
                return 613;
            }

            bool newWatcherAdded = await ServiceDiscovery_HA_TestHelper.WaitForProxyHasService(newWatcherProxy, newWatcherTimer,
                providerSceneName, 6000);
            if (!newWatcherAdded)
            {
                Log.Console("metadata replay new watcher did not receive add for new role");
                return 614;
            }

            bool roleUpdated = await WaitForProxyServiceRole(newWatcherProxy, newWatcherTimer, providerSceneName, "ProviderV2", 3000);
            if (!roleUpdated)
            {
                Log.Console("metadata replay new watcher role is not ProviderV2");
                return 615;
            }

            await testFiber.RemoveFiber(providerFiber.Id);
            await testFiber.RemoveFiber(oldWatcherFiber.Id);
            await testFiber.RemoveFiber(newWatcherFiber.Id);
            await testFiber.RemoveFiber(nodeB.Id);

            Log.Console("ServiceDiscovery FailoverMetadataChangeReplay passed");
            return ErrorCode.ERR_Success;
        }

        private static async ETTask<bool> WaitForProxyServiceRole(ServiceDiscoveryProxy proxy, TimerComponent timer, string sceneName,
            string role, long timeoutMs)
        {
            if (proxy == null || timer == null || string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(role))
            {
                return false;
            }

            EntityRef<ServiceDiscoveryProxy> proxyRef = proxy;
            EntityRef<TimerComponent> timerRef = timer;
            long deadline = timer.GetSingleton<TimeInfo>().ServerNow() + timeoutMs;
            while (timer.GetSingleton<TimeInfo>().ServerNow() <= deadline)
            {
                proxy = proxyRef;
                if (proxy == null)
                {
                    return false;
                }

                if (proxy.SceneNameServices.TryGetValue(sceneName, out EntityRef<ServiceInfo> serviceRef))
                {
                    ServiceInfo serviceInfo = serviceRef;
                    if (serviceInfo != null &&
                        serviceInfo.Metadata != null &&
                        serviceInfo.Metadata.TryGetValue("Role", out string metadataRole) &&
                        metadataRole == role)
                    {
                        return true;
                    }
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

        private static bool TrySetAgentLocalServiceRole(Fiber testFiber, string sceneName, string role)
        {
            if (testFiber == null || string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(role))
            {
                return false;
            }

            Fiber agentFiber = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryAgentFiber(testFiber);
            ServiceDiscoveryAgent agent = agentFiber.Root.GetComponent<ServiceDiscoveryAgent>();
            if (!agent.LocalPublishedServices.TryGetValue(sceneName, out (ActorId ActorId, StringKV Metadata) localService))
            {
                return false;
            }

            StringKV metadata = localService.Metadata == null ? new StringKV() : new StringKV(localService.Metadata);
            metadata["Role"] = role;
            agent.LocalPublishedServices[sceneName] = (localService.ActorId, metadata);
            return true;
        }
    }
}
