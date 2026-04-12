using ET.Server;

namespace ET.Test
{
    [Event(SceneType.All)]
    public class Servicediscovery_AddService_NotifyWait : AEvent<Scene, OnServiceChangeAddService>
    {
        protected override async ETTask Run(Scene scene, OnServiceChangeAddService args)
        {
            ServiceDiscoveryTestAddEventCounterComponent counter = scene.GetComponent<ServiceDiscoveryTestAddEventCounterComponent>();
            if (counter != null && !string.IsNullOrEmpty(args.ServiceName))
            {
                counter.Counts.TryGetValue(args.ServiceName, out int count);
                counter.Counts[args.ServiceName] = count + 1;
            }

            await ETTask.CompletedTask;
        }
    }

    [SkipAwaitEntityCheck]
    /// <summary>
    /// 订阅通知准确性测试：
    /// 验证订阅返回即拿到当前快照、不会重复触发初始 add，并验证删除通知准确下发。
    /// </summary>
    public class Servicediscovery_SubscriptionNotificationAccuracy_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            // 1. 准备测试环境

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady(context.Fiber);
            if (addressError != 0)
            {
                Log.Console($"subscription ensure address singleton failed: {addressError}");
                return 2;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_SubscriptionNotificationAccuracy_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"subscription reset storage failed: {resetError}");
                return 1;
            }

            Fiber node = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_Subscription");
            ServiceDiscovery sd = node.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timer = node.Root.TimerComponent;
            bool isMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sd, timer, 5000);
            if (!isMaster)
            {
                Log.Console("subscription node is not master");
                return 5;
            }

            // 2. 先注册 Provider/Client，确保订阅响应走“即时快照”路径。
            Fiber providerFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "Provider_Accuracy",
                new StringKV { { "Role", "Provider" } });
            Fiber clientFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "Client_Accuracy",
                new StringKV { { "Role", "Client" } });
            if (providerFiber == null || clientFiber == null)
            {
                Log.Console("subscription create provider/client fibers failed");
                return 6;
            }

            string providerSceneName = providerFiber.Root.Name;
            string clientSceneName = clientFiber.Root.Name;

            // 3. 拉起两个订阅者，分别订阅 Provider 与 Client。
            Fiber subAFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "SubA_Accuracy",
                new StringKV { { "Role", "WatcherA" } });
            Fiber subBFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "SubB_Accuracy",
                new StringKV { { "Role", "WatcherB" } });
            if (subAFiber == null || subBFiber == null)
            {
                Log.Console("subscription create subscriber fibers failed");
                return 7;
            }

            ServiceDiscoveryProxy subAProxy = subAFiber.Root.GetComponent<ServiceDiscoveryProxy>();
            ServiceDiscoveryProxy subBProxy = subBFiber.Root.GetComponent<ServiceDiscoveryProxy>();
            TimerComponent subATimer = subAFiber.Root.TimerComponent;
            TimerComponent subBTimer = subBFiber.Root.TimerComponent;
            ServiceDiscoveryTestAddEventCounterComponent subACounter =
                    subAFiber.Root.AddComponent<ServiceDiscoveryTestAddEventCounterComponent>();

            subACounter.Counts.Clear();
            await subAProxy.SubscribeServiceChange("RoleProvider", new StringKV { { "Role", "Provider" } });
            subACounter.Counts.TryGetValue(providerSceneName, out int subAAddCount);
            if (subAAddCount != 1)
            {
                Log.Console($"subscription subA initial add count invalid, actual: {subAAddCount}, expected: 1");
                return 9;
            }

            await subBProxy.SubscribeServiceChange("RoleClient", new StringKV { { "Role", "Client" } });

            if (!HasLocalService(subAProxy, providerSceneName))
            {
                Log.Console("subscription subA did not receive provider snapshot synchronously");
                return 10;
            }

            if (!HasLocalService(subBProxy, clientSceneName))
            {
                Log.Console("subscription subB did not receive client snapshot synchronously");
                return 11;
            }

            if (HasLocalService(subAProxy, clientSceneName))
            {
                Log.Console("subscription subA unexpectedly received client snapshot");
                return 12;
            }

            if (HasLocalService(subBProxy, providerSceneName))
            {
                Log.Console("subscription subB unexpectedly received provider snapshot");
                return 13;
            }

            await subATimer.WaitAsync(1200);
            subACounter.Counts.TryGetValue(providerSceneName, out int subAAddCountAfterWindow);
            if (subAAddCountAfterWindow != 1)
            {
                Log.Console($"subscription subA received duplicate initial add, count: {subAAddCountAfterWindow}");
                return 14;
            }

            // 4. 注销 Provider，验证删除通知准确性。
            ServiceDiscoveryProxy providerProxy = providerFiber.Root.GetComponent<ServiceDiscoveryProxy>();
            await providerProxy.UnregisterFromServiceDiscovery();
            bool subAProviderRemoved = await ServiceDiscovery_HA_TestHelper.WaitForProxyNotHasService(subAProxy, subATimer,
                providerSceneName, 5000);
            if (!subAProviderRemoved)
            {
                Log.Console("subscription subA did not receive provider remove notification");
                return 16;
            }

            if (HasLocalService(subBProxy, providerSceneName))
            {
                Log.Console("subscription subB provider state invalid after remove");
                return 17;
            }

            // 测试成功路径显式控制释放顺序：先释放业务 proxy，再释放 service discovery。
            await testFiber.RemoveFiber(providerFiber.Id);
            await testFiber.RemoveFiber(clientFiber.Id);
            await testFiber.RemoveFiber(subAFiber.Id);
            await testFiber.RemoveFiber(subBFiber.Id);
            await testFiber.RemoveFiber(node.Id);

            Log.Console("ServiceDiscovery SubscriptionNotificationAccuracy passed");
            return ErrorCode.ERR_Success;
        }

        private static bool HasLocalService(ServiceDiscoveryProxy proxy, string sceneName)
        {
            if (proxy == null || string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

            if (!proxy.SceneNameServices.TryGetValue(sceneName, out EntityRef<ServiceInfo> serviceRef))
            {
                return false;
            }

            ServiceInfo serviceInfo = serviceRef;
            return serviceInfo != null;
        }
    }
}
