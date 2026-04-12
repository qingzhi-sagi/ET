using System;
using System.Collections.Generic;
using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 并发选举测试：
    /// 同时触发多个 ServiceDiscovery 节点抢主，验证最终只有一个 Master。
    /// </summary>
    public class Servicediscovery_ConcurrentElection_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            // 1. 准备测试环境（清库 + 地址初始化）

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady();
            if (addressError != 0)
            {
                Log.Console($"concurrent election ensure address singleton failed: {addressError}");
                return 111;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_ConcurrentElection_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"concurrent election reset storage failed: {resetError}");
                return 100;
            }

            List<StartSceneConfig> serviceDiscoveryConfigs = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryConfigs();
            if (serviceDiscoveryConfigs.Count == 0)
            {
                Log.Console("concurrent election no service discovery config found");
                return 112;
            }

            // 2. 拉起多个 ServiceDiscovery 节点（含配置主备 + 额外节点）
            List<Fiber> nodes = new();
            Fiber primaryNode = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                serviceDiscoveryConfigs[0].Name);
            nodes.Add(primaryNode);

            if (serviceDiscoveryConfigs.Count > 1)
            {
                Fiber standbyNode = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 1,
                    serviceDiscoveryConfigs[1].Name);
                nodes.Add(standbyNode);
            }

            const int desiredNodeCount = 5;
            for (int i = nodes.Count; i < desiredNodeCount; ++i)
            {
                string extraSceneName = $"ServiceDiscovery_Election_{i:00}";
                Fiber node = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, i, extraSceneName);
                nodes.Add(node);
            }

            // 3. 并发触发抢主流程
            TimerComponent timer = nodes[0].Root.TimerComponent;
            ETTask<bool>[] electionTasks = new ETTask<bool>[nodes.Count];
            for (int i = 0; i < nodes.Count; ++i)
            {
                ServiceDiscovery sd = nodes[i].Root.GetComponent<ServiceDiscovery>();
                sd.GetOrAddLease().MasterLeaseTimeout = ServiceDiscovery_HA_TestHelper.FastLeaseTimeoutMs;
                sd.GetOrAddLease().MasterLeaseRenewInterval = ServiceDiscovery_HA_TestHelper.FastLeaseRenewIntervalMs;
                sd.GetOrAddLease().CurrentMasterLeaseExpireTime = 0;
                electionTasks[i] = sd.EnsureActiveMasterAsync();
            }

            for (int i = 0; i < electionTasks.Length; ++i)
            {
                try
                {
                    await electionTasks[i];
                }
                catch (Exception e)
                {
                    Log.Debug($"concurrent election ensure active master throws, index: {i}, msg: {e.Message}");
                }
            }

            // 4. 验证唯一主与主记录一致性（scene/actor/epoch/lease）
            (int waitError, string masterSceneName, ActorId masterActorId, int masterCount) =
                await ServiceDiscovery_HA_TestHelper.WaitSingleMaster(nodes, timer, 8000);
            if (waitError != 0)
            {
                Log.Console($"concurrent election wait single master failed, error: {waitError}");
                return 103;
            }

            if (masterCount != 1)
            {
                Log.Console($"concurrent election expected one master, actual: {masterCount}");
                return 104;
            }

            ServiceDiscovery anyNodeSd = nodes[0].Root.GetComponent<ServiceDiscovery>();
            if (anyNodeSd == null)
            {
                Log.Console("concurrent election any node service discovery is null");
                return 105;
            }

            (int recordError, string sceneName, ActorId actorId, long epoch, long leaseExpireTime) =
                await ServiceDiscovery_HA_TestHelper.QueryMasterRecord(anyNodeSd);
            if (recordError != 0)
            {
                Log.Console($"concurrent election query master record failed, error: {recordError}");
                return 106;
            }

            if (sceneName != masterSceneName || actorId != masterActorId)
            {
                Log.Console(
                    $"concurrent election master mismatch, expected: {masterSceneName}/{masterActorId}, actual: {sceneName}/{actorId}");
                return 107;
            }

            if (epoch <= 0)
            {
                Log.Console($"concurrent election master epoch invalid: {epoch}");
                return 108;
            }

            if (leaseExpireTime <= anyNodeSd.GetSingleton<TimeInfo>().ServerNow())
            {
                Log.Console($"concurrent election master lease expired: {leaseExpireTime}");
                return 109;
            }

            Log.Console(
                $"ServiceDiscovery ConcurrentElection passed, master: {masterSceneName}, actor: {masterActorId}, epoch: {epoch}");
            return ErrorCode.ERR_Success;
        }
    }
}
