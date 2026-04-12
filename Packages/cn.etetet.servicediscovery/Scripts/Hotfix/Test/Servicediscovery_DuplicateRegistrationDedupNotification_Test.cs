using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 重复注册去重通知测试：
    /// 相同服务以相同 ActorId/Metadata 重复注册时，不应再次进入待发送通知缓冲区。
    /// </summary>
    public class Servicediscovery_DuplicateRegistrationDedupNotification_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady(context.Fiber);
            if (addressError != 0)
            {
                Log.Console($"duplicate register ensure address singleton failed: {addressError}");
                return 2;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_DuplicateRegistrationDedupNotification_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"duplicate register reset storage failed: {resetError}");
                return 1;
            }

            Fiber node = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_DuplicateRegister");
            ServiceDiscovery sd = node.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timer = node.Root.TimerComponent;
            bool isMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sd, timer, 5000);
            if (!isMaster)
            {
                Log.Console("duplicate register node is not master");
                return 5;
            }

            Fiber watcherFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "Watcher_Dedup",
                new StringKV { { "Role", "Watcher" } });
            if (watcherFiber == null)
            {
                Log.Console("duplicate register create watcher fiber failed");
                return 6;
            }

            ServiceDiscoveryProxy watcherProxy = watcherFiber.Root.GetComponent<ServiceDiscoveryProxy>();
            TimerComponent watcherTimer = watcherFiber.Root.TimerComponent;
            ServiceDiscoveryTestAddEventCounterComponent watcherCounter =
                    watcherFiber.Root.AddComponent<ServiceDiscoveryTestAddEventCounterComponent>();

            watcherCounter.Counts.Clear();
            await watcherProxy.SubscribeServiceChange("RoleProvider", new StringKV { { "Role", "Provider" } });

            Fiber providerFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "Provider_Dedup",
                new StringKV { { "Role", "Provider" } });
            if (providerFiber == null)
            {
                Log.Console("duplicate register create provider fiber failed");
                return 8;
            }

            ServiceDiscoveryProxy providerProxy = providerFiber.Root.GetComponent<ServiceDiscoveryProxy>();
            bool watcherHasProvider = await ServiceDiscovery_HA_TestHelper.WaitForProxyHasService(watcherProxy, watcherTimer,
                providerFiber.Root.Name, 5000);
            if (!watcherHasProvider)
            {
                Log.Console("duplicate register watcher did not receive initial provider notification");
                return 10;
            }

            watcherCounter.Counts.TryGetValue(providerFiber.Root.Name, out int initialAddCount);
            if (initialAddCount != 1)
            {
                Log.Console($"duplicate register initial add count invalid, actual: {initialAddCount}");
                return 13;
            }

            bool notificationBufferCleared = await WaitForPendingNotificationsEmpty(sd, timer, 3000);
            if (!notificationBufferCleared)
            {
                Log.Console("duplicate register notification buffer did not drain after initial registration");
                return 11;
            }

            ServiceDiscoveryNotificationBufferComponent notificationBuffer = sd.GetOrAddNotificationBuffer();
            notificationBuffer.NotificationDebounceInterval = 10_000;

            await providerProxy.RegisterToServiceDiscovery(new StringKV { { "Role", "Provider" } });

            if (notificationBuffer.PendingNotifications.Count != 0)
            {
                Log.Console($"duplicate register should not enqueue notifications, actual pending targets: {notificationBuffer.PendingNotifications.Count}");
                return 12;
            }

            await watcherTimer.WaitAsync(1200);
            watcherCounter.Counts.TryGetValue(providerFiber.Root.Name, out int addCountAfterDuplicateRegister);
            if (addCountAfterDuplicateRegister != 1)
            {
                Log.Console($"duplicate register should not trigger duplicate add event, actual count: {addCountAfterDuplicateRegister}");
                return 14;
            }

            if (!watcherProxy.SceneNameServices.TryGetValue(providerFiber.Root.Name, out EntityRef<ServiceInfo> watcherServiceRef))
            {
                Log.Console("duplicate register watcher local cache lost provider after noop register");
                return 15;
            }

            ServiceInfo watcherServiceInfo = watcherServiceRef;
            if (watcherServiceInfo == null || watcherServiceInfo.ActorId != providerFiber.Root.GetActorId())
            {
                Log.Console("duplicate register watcher local cache actor changed unexpectedly");
                return 16;
            }

            if (!watcherServiceInfo.Metadata.TryGetValue("Role", out string role) || role != "Provider")
            {
                Log.Console("duplicate register watcher local cache metadata changed unexpectedly");
                return 17;
            }

            await testFiber.RemoveFiber(providerFiber.Id);
            await testFiber.RemoveFiber(watcherFiber.Id);
            await testFiber.RemoveFiber(node.Id);

            Log.Console("ServiceDiscovery DuplicateRegistrationDedupNotification passed");
            return ErrorCode.ERR_Success;
        }

        private static async ETTask<bool> WaitForPendingNotificationsEmpty(ServiceDiscovery sd, TimerComponent timer, long timeoutMs)
        {
            if (sd == null || timer == null)
            {
                return false;
            }

            EntityRef<ServiceDiscovery> sdRef = sd;
            EntityRef<TimerComponent> timerRef = timer;
            long deadline = timer.GetSingleton<TimeInfo>().ServerNow() + timeoutMs;
            while (timer.GetSingleton<TimeInfo>().ServerNow() <= deadline)
            {
                sd = sdRef;
                if (sd == null)
                {
                    return false;
                }

                if (sd.GetOrAddNotificationBuffer().PendingNotifications.Count == 0)
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
