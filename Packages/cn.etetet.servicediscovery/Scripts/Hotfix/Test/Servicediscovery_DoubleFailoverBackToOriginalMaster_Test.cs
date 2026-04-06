using System.Collections.Generic;
using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 双次切换回原主测试：
    /// 验证 A 挂掉后 B 接管；A 重建后 B 再挂掉，最终重新切换回 A。
    /// </summary>
    public class Servicediscovery_DoubleFailoverBackToOriginalMaster_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            // 1. 准备测试环境（清库 + 地址初始化）

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady();
            if (addressError != 0)
            {
                Log.Console($"double failover ensure address singleton failed: {addressError}");
                return 611;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_DoubleFailoverBackToOriginalMaster_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"double failover reset storage failed: {resetError}");
                return 600;
            }

            // 2. 拉起 A/B 两个服务发现节点
            Fiber nodeA = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_DoubleFailover_A");
            Fiber nodeB = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 1,
                "ServiceDiscovery_DoubleFailover_B");
            ServiceDiscovery sdA = nodeA.Root.GetComponent<ServiceDiscovery>();
            ServiceDiscovery sdB = nodeB.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timerA = nodeA.Root.TimerComponent;
            TimerComponent timerB = nodeB.Root.TimerComponent;
            int leasePrepareError = await ServiceDiscovery_HA_TestHelper.ConfigureFastLease(sdA, sdB, timerB);
            if (leasePrepareError != 0)
            {
                Log.Console($"double failover fast lease prepare failed: {leasePrepareError}");
                return 603;
            }

            bool aMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdA, timerA, 5000);
            if (!aMaster)
            {
                Log.Console("double failover nodeA not master at start");
                return 604;
            }

            (int firstRecordError, string firstScene, ActorId firstActorId, long firstEpoch, _) =
                await ServiceDiscovery_HA_TestHelper.QueryMasterRecord(sdA);
            if (firstRecordError != 0)
            {
                Log.Console($"double failover query first master record failed: {firstRecordError}");
                return 612;
            }

            if (firstScene != nodeA.Root.Name || firstActorId != nodeA.Root.GetActorId())
            {
                Log.Console(
                    $"double failover first master mismatch, expected: {nodeA.Root.Name}/{nodeA.Root.GetActorId()}, actual: {firstScene}/{firstActorId}");
                return 613;
            }

            if (firstEpoch <= 0)
            {
                Log.Console($"double failover first epoch invalid: {firstEpoch}");
                return 614;
            }

            // 3. 模拟 A 挂掉，等待 B 接管
            await testFiber.RemoveFiber(nodeA.Id);
            bool bTakeover = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdB, timerB, 10000);
            if (!bTakeover)
            {
                Log.Console("double failover nodeB takeover timeout after nodeA down");
                return 605;
            }

            (int secondRecordError, string secondScene, ActorId secondActorId, long secondEpoch, _) =
                await ServiceDiscovery_HA_TestHelper.QueryMasterRecord(sdB);
            if (secondRecordError != 0)
            {
                Log.Console($"double failover query second master record failed: {secondRecordError}");
                return 606;
            }

            if (secondScene != nodeB.Root.Name || secondActorId != nodeB.Root.GetActorId())
            {
                Log.Console(
                    $"double failover second master mismatch, expected: {nodeB.Root.Name}/{nodeB.Root.GetActorId()}, actual: {secondScene}/{secondActorId}");
                return 607;
            }

            if (secondEpoch <= firstEpoch)
            {
                Log.Console($"double failover epoch not advanced after first failover, first: {firstEpoch}, second: {secondEpoch}");
                return 608;
            }

            // 4. 重建 A，确认 B 仍保持主（避免重建后直接脑裂）
            Fiber nodeARecovered = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_DoubleFailover_A_Recovered");
            ServiceDiscovery sdARecovered = nodeARecovered.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timerARecovered = nodeARecovered.Root.TimerComponent;
            sdARecovered.GetOrAddLease().MasterLeaseTimeout = ServiceDiscovery_HA_TestHelper.FastLeaseTimeoutMs;
            sdARecovered.GetOrAddLease().MasterLeaseRenewInterval = ServiceDiscovery_HA_TestHelper.FastLeaseRenewIntervalMs;
            sdB.GetOrAddLease().MasterLeaseTimeout = ServiceDiscovery_HA_TestHelper.FastLeaseTimeoutMs;
            sdB.GetOrAddLease().MasterLeaseRenewInterval = ServiceDiscovery_HA_TestHelper.FastLeaseRenewIntervalMs;

            (int settleError, string settleMasterScene, _, int settleMasterCount) =
                await ServiceDiscovery_HA_TestHelper.WaitSingleMaster(new List<Fiber> { nodeB, nodeARecovered }, timerB, 8000);
            if (settleError != 0 || settleMasterCount != 1)
            {
                Log.Console(
                    $"double failover settle single master failed, error: {settleError}, masterCount: {settleMasterCount}");
                return 617;
            }

            if (settleMasterScene != nodeB.Root.Name)
            {
                Log.Console($"double failover unexpected master after nodeA recreate, expected: {nodeB.Root.Name}, actual: {settleMasterScene}");
                return 618;
            }

            // 5. 模拟 B 挂掉，验证重新切换回 A
            await testFiber.RemoveFiber(nodeB.Id);
            bool aTakeoverAgain = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdARecovered, timerARecovered, 10000);
            if (!aTakeoverAgain)
            {
                Log.Console("double failover nodeA takeover timeout after nodeB down");
                return 609;
            }

            (int finalRecordError, string finalScene, ActorId finalActorId, long finalEpoch, long finalLeaseExpireTime) =
                await ServiceDiscovery_HA_TestHelper.QueryMasterRecord(sdARecovered);
            if (finalRecordError != 0)
            {
                Log.Console($"double failover query final master record failed: {finalRecordError}");
                return 610;
            }

            if (finalScene != nodeARecovered.Root.Name || finalActorId != nodeARecovered.Root.GetActorId())
            {
                Log.Console(
                    $"double failover final master mismatch, expected: {nodeARecovered.Root.Name}/{nodeARecovered.Root.GetActorId()}, actual: {finalScene}/{finalActorId}");
                return 619;
            }

            if (finalEpoch <= secondEpoch)
            {
                Log.Console($"double failover final epoch not advanced, second: {secondEpoch}, final: {finalEpoch}");
                return 620;
            }

            if (finalLeaseExpireTime <= TimeInfo.Instance.ServerNow())
            {
                Log.Console($"double failover final lease expired: {finalLeaseExpireTime}");
                return 621;
            }

            Log.Console(
                $"ServiceDiscovery DoubleFailoverBack passed, epoch: {firstEpoch}->{secondEpoch}->{finalEpoch}, finalMaster: {finalScene}");
            return ErrorCode.ERR_Success;
        }
    }
}
