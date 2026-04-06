using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 验证按 Zone 过滤的订阅在“先订阅、后注册”场景下可以收到新增服务通知。
    /// </summary>
    public class Servicediscovery_ZoneFilteredLateRegistration_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady();
            if (addressError != 0)
            {
                Log.Console($"zone filtered late registration ensure address singleton failed: {addressError}");
                return 1;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_ZoneFilteredLateRegistration_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"zone filtered late registration reset storage failed: {resetError}");
                return 2;
            }

            Fiber node = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_ZoneFilteredLateRegistration");
            ServiceDiscovery master = node.Root.GetComponent<ServiceDiscovery>();
            TimerComponent masterTimer = node.Root.TimerComponent;
            bool isMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(master, masterTimer, 5000);
            if (!isMaster)
            {
                Log.Console("zone filtered late registration node is not master");
                return 5;
            }

            Fiber subscriberFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "Subscriber_ZoneFilter",
                new StringKV { { "Role", "Subscriber" } });
            if (subscriberFiber == null)
            {
                Log.Console("zone filtered late registration create subscriber fiber failed");
                return 6;
            }

            ServiceDiscoveryProxy subscriberProxy = subscriberFiber.Root.GetComponent<ServiceDiscoveryProxy>();
            TimerComponent subscriberTimer = subscriberFiber.Root.TimerComponent;
            string sceneTypeName = SceneTypeSingleton.Instance.GetSceneName(SceneType.TestEmpty);
            int zone = subscriberFiber.Zone;
            await subscriberProxy.SubscribeServiceChange("ZoneScopedProvider", new StringKV
            {
                { ServiceMetaKey.SceneType, sceneTypeName },
                { ServiceMetaKey.Zone, zone.ToString() },
                { "Role", "Provider" }
            });

            Fiber providerFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "Provider_ZoneFilter",
                new StringKV { { "Role", "Provider" } });
            if (providerFiber == null)
            {
                Log.Console("zone filtered late registration create provider fiber failed");
                return 8;
            }

            string providerSceneName = providerFiber.Root.Name;
            bool subscriberReceivedProvider = await ServiceDiscovery_HA_TestHelper.WaitForProxyHasService(subscriberProxy,
                subscriberTimer, providerSceneName, 5000);
            if (!subscriberReceivedProvider)
            {
                Log.Console("zone filtered late registration subscriber did not receive provider add notification");
                return 9;
            }

            if (!master.Services.TryGetValue(providerSceneName, out EntityRef<ServiceInfo> providerServiceRef))
            {
                Log.Console("zone filtered late registration master missing provider service");
                return 10;
            }

            ServiceInfo providerService = providerServiceRef;
            if (providerService == null)
            {
                Log.Console("zone filtered late registration provider service disposed unexpectedly");
                return 11;
            }

            if (!providerService.Metadata.TryGetValue(ServiceMetaKey.Zone, out string zoneValue))
            {
                Log.Console("zone filtered late registration provider metadata missing zone");
                return 12;
            }

            if (zoneValue != providerFiber.Zone.ToString())
            {
                Log.Console(
                    $"zone filtered late registration provider zone metadata invalid actual: {zoneValue} expected: {providerFiber.Zone}");
                return 13;
            }

            await testFiber.RemoveFiber(providerFiber.Id);
            await testFiber.RemoveFiber(subscriberFiber.Id);
            await testFiber.RemoveFiber(node.Id);

            Log.Console("ServiceDiscovery ZoneFilteredLateRegistration passed");
            return ErrorCode.ERR_Success;
        }
    }
}
