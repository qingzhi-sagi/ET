using System;
using System.Collections.Generic;
using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 性能与切换时延测试：
    /// 验证大规模注册、心跳处理性能，以及主备切换时长。
    /// </summary>
    public class Servicediscovery_PerformanceAndSwitchTime_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            // 1. 准备测试环境

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady();
            if (addressError != 0)
            {
                Log.Console($"performance ensure address singleton failed: {addressError}");
                return 411;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_PerformanceAndSwitchTime_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"performance reset storage failed: {resetError}");
                return 400;
            }

            List<StartSceneConfig> serviceDiscoveryConfigs = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryConfigs();
            if (serviceDiscoveryConfigs.Count == 0)
            {
                Log.Console("performance no service discovery config found");
                return 412;
            }

            // 2. 拉起主备并确认主节点可写
            Fiber nodeA = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                serviceDiscoveryConfigs[0].Name);
            Fiber nodeB = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 1,
                "ServiceDiscovery_PerfFollower");
            ServiceDiscovery sdA = nodeA.Root.GetComponent<ServiceDiscovery>();
            ServiceDiscovery sdB = nodeB.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timerB = nodeB.Root.TimerComponent;
            int leasePrepareError = await ServiceDiscovery_HA_TestHelper.ConfigureFastLease(sdA, sdB, timerB);
            if (leasePrepareError != 0)
            {
                Log.Console($"performance fast lease prepare failed: {leasePrepareError}");
                return 403;
            }

            bool aMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdA, timerB, 5000);
            if (!aMaster)
            {
                int forceExpireError = await ServiceDiscovery_HA_TestHelper.ForceMasterLeaseExpireIfNotSelf(sdA);
                if (forceExpireError == 0)
                {
                    aMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdA, timerB, 5000);
                }

                if (!aMaster)
                {
                    Log.Console("performance nodeA master wait failed");
                    return 404;
                }
            }

            // 3. 执行 1200 服务注册并验证持久化数量
            const int serviceCount = 1200;
            string perfPrefix = "PerfService_";
            Address address = nodeA.Root.GetActorId().Address;
            StringKV perfMetadata = new()
            {
                { ServiceMetaKey.SceneType, "PerfProbe" },
                { "Category", "Performance" },
            };

            long registerBegin = TimeInfo.Instance.ServerNow();
            for (int i = 0; i < serviceCount; ++i)
            {
                string sceneName = $"{perfPrefix}{i}";
                ActorId actorId = new(address, new FiberInstanceId(700000 + i));
                await sdA.RegisterServiceAsync(sceneName, actorId, perfMetadata);
            }

            long registerCost = TimeInfo.Instance.ServerNow() - registerBegin;
            int perfServiceCount = 0;
            foreach ((string sceneName, EntityRef<ServiceInfo> serviceRef) in sdA.Services)
            {
                ServiceInfo serviceInfo = serviceRef;
                if (serviceInfo != null && !string.IsNullOrEmpty(sceneName) &&
                    sceneName.StartsWith(perfPrefix, StringComparison.Ordinal))
                {
                    ++perfServiceCount;
                }
            }

            if (perfServiceCount != serviceCount)
            {
                Log.Console($"performance service count mismatch, expected: {serviceCount}, actual: {perfServiceCount}");
                return 406;
            }

            // 4. 执行 1200 心跳并验证“心跳不改服务实体”语义
            if (!sdA.Services.TryGetValue($"{perfPrefix}0", out EntityRef<ServiceInfo> sampleRef))
            {
                Log.Console("performance sample service is missing");
                return 407;
            }

            ServiceInfo sampleBefore = sampleRef;
            if (sampleBefore == null)
            {
                Log.Console("performance sampleBefore entity is null");
                return 407;
            }

            long heartbeatServiceTimeBefore = sampleBefore.LastHeartbeatTime;

            long heartbeatBegin = TimeInfo.Instance.ServerNow();
            for (int i = 0; i < serviceCount; ++i)
            {
                ActorId actorId = new(address, new FiberInstanceId(700000 + i));
                await sdA.UpdateAgentHeartbeatAsync(actorId);
            }

            long heartbeatCost = TimeInfo.Instance.ServerNow() - heartbeatBegin;
            ServiceInfo sampleAfter = sampleRef;
            if (sampleAfter == null)
            {
                Log.Console("performance sampleAfter entity is null");
                return 408;
            }

            if (sampleAfter.LastHeartbeatTime != heartbeatServiceTimeBefore)
            {
                Log.Console(
                    $"performance heartbeat unexpectedly mutated service entity, before: {heartbeatServiceTimeBefore}, after: {sampleAfter.LastHeartbeatTime}");
                return 409;
            }

            if (heartbeatCost > 8000)
            {
                Log.Console($"performance heartbeat cost too high, cost: {heartbeatCost}ms");
                return 410;
            }

            // 5. 模拟主节点故障并验证接管耗时
            long removeBegin = TimeInfo.Instance.ServerNow();
            await testFiber.RemoveFiber(nodeA.Id);
            long removeCost = TimeInfo.Instance.ServerNow() - removeBegin;

            long switchBegin = TimeInfo.Instance.ServerNow();

            bool bTakeover = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdB, timerB, 10000);
            if (!bTakeover)
            {
                int forceExpireError = await ServiceDiscovery_HA_TestHelper.ForceMasterLeaseExpireIfNotSelf(sdB);
                if (forceExpireError == 0)
                {
                    bTakeover = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdB, timerB, 5000);
                }

                if (!bTakeover)
                {
                    Log.Console("performance switch wait nodeB master timeout");
                    return 413;
                }
            }

            long switchCost = TimeInfo.Instance.ServerNow() - switchBegin;
            if (switchCost > 6000)
            {
                Log.Console($"performance switch too slow, cost: {switchCost}ms");
                return 414;
            }

            Log.Console(
                $"ServiceDiscovery Performance passed, registerCost: {registerCost}ms, heartbeatCost: {heartbeatCost}ms, removeCost: {removeCost}ms, switchCost: {switchCost}ms, services: {serviceCount}");
            return ErrorCode.ERR_Success;
        }
    }
}
