using System.Collections.Generic;
using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 脑裂收敛测试：
    /// 人为制造双主状态，验证系统最终收敛为单主并与主记录一致。
    /// </summary>
    public class Servicediscovery_SplitBrainResolution_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            // 1. 准备测试环境

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady();
            if (addressError != 0)
            {
                Log.Console($"split brain ensure address singleton failed: {addressError}");
                return 311;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_SplitBrainResolution_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"split brain reset storage failed: {resetError}");
                return 300;
            }

            List<StartSceneConfig> serviceDiscoveryConfigs = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryConfigs();
            if (serviceDiscoveryConfigs.Count == 0)
            {
                Log.Console("split brain no service discovery config found");
                return 312;
            }

            // 2. 拉起双节点并确认初始为单主
            Fiber nodeA = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                serviceDiscoveryConfigs[0].Name);
            Fiber nodeB = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 1,
                "ServiceDiscovery_SplitFollower");
            ServiceDiscovery sdA = nodeA.Root.GetComponent<ServiceDiscovery>();
            ServiceDiscovery sdB = nodeB.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timer = nodeA.Root.TimerComponent;
            int leasePrepareError = await ServiceDiscovery_HA_TestHelper.ConfigureFastLease(sdA, sdB, timer);
            if (leasePrepareError != 0)
            {
                Log.Console($"split brain fast lease prepare failed: {leasePrepareError}");
                return 303;
            }

            (int waitError, _, _, int masterCount) =
                await ServiceDiscovery_HA_TestHelper.WaitSingleMaster(new List<Fiber> { nodeA, nodeB }, timer, 8000);
            if (waitError != 0 || masterCount != 1)
            {
                Log.Console($"split brain initial master state invalid, waitError: {waitError}, masterCount: {masterCount}");
                return 304;
            }

            // 3. 手工注入“双主”状态并触发主租约校验
            long now = testFiber.GetSingleton<TimeInfo>().ServerNow();
            sdA.GetOrAddLease().IsActiveMaster = true;
            sdA.GetOrAddLease().CurrentMasterSceneName = nodeA.Root.Name;
            sdA.GetOrAddLease().CurrentMasterActorId = nodeA.Root.GetActorId();
            sdA.GetOrAddLease().CurrentMasterLeaseExpireTime = now;

            sdB.GetOrAddLease().IsActiveMaster = true;
            sdB.GetOrAddLease().CurrentMasterSceneName = nodeB.Root.Name;
            sdB.GetOrAddLease().CurrentMasterActorId = nodeB.Root.GetActorId();
            sdB.GetOrAddLease().CurrentMasterLeaseExpireTime = now;

            ETTask<bool> ensureA = sdA.EnsureActiveMasterAsync();
            ETTask<bool> ensureB = sdB.EnsureActiveMasterAsync();
            bool aMaster = await ensureA;
            bool bMaster = await ensureB;

            if (aMaster && bMaster)
            {
                Log.Console("split brain both nodes still master after convergence");
                return 305;
            }

            // 4. 验证最终收敛与数据库主记录一致
            (int convergeError, string masterSceneName, ActorId masterActorId, int convergeCount) =
                await ServiceDiscovery_HA_TestHelper.WaitSingleMaster(new List<Fiber> { nodeA, nodeB }, timer, 8000);
            if (convergeError != 0 || convergeCount != 1)
            {
                Log.Console($"split brain convergence failed, error: {convergeError}, masterCount: {convergeCount}");
                return 306;
            }

            (int recordError, string sceneName, ActorId actorId, _, _) = await ServiceDiscovery_HA_TestHelper.QueryMasterRecord(sdA);
            if (recordError != 0)
            {
                Log.Console($"split brain query master record failed: {recordError}");
                return 307;
            }

            if (sceneName != masterSceneName || actorId != masterActorId)
            {
                Log.Console(
                    $"split brain record mismatch, expected: {masterSceneName}/{masterActorId}, actual: {sceneName}/{actorId}");
                return 308;
            }

            Log.Console($"ServiceDiscovery SplitBrain passed, master: {masterSceneName}, actor: {masterActorId}");
            return ErrorCode.ERR_Success;
        }
    }
}
