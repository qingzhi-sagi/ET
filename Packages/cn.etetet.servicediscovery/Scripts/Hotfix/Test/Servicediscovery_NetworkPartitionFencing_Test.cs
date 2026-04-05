using System;
using System.Collections.Generic;
using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 网络分区与 Fencing 测试：
    /// 模拟旧主与 Mongo 断联，验证备机接管后旧主写入被拒绝。
    /// </summary>
    public class Servicediscovery_NetworkPartitionFencing_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            // 1. 准备测试环境

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady();
            if (addressError != 0)
            {
                Log.Console($"network partition ensure address singleton failed: {addressError}");
                return 211;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_NetworkPartitionFencing_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"network partition reset storage failed: {resetError}");
                return 200;
            }

            List<StartSceneConfig> serviceDiscoveryConfigs = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryConfigs();
            if (serviceDiscoveryConfigs.Count == 0)
            {
                Log.Console("network partition no service discovery config found");
                return 212;
            }

            // 2. 拉起主备节点并进入快速租约模式
            Fiber nodeA = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                serviceDiscoveryConfigs[0].Name);
            Fiber nodeB = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 1,
                "ServiceDiscovery_NetworkFollower");
            if (nodeA == null || nodeB == null)
            {
                Log.Console("network partition create nodes failed");
                return 201;
            }

            ServiceDiscovery sdA = nodeA.Root.GetComponent<ServiceDiscovery>();
            ServiceDiscovery sdB = nodeB.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timerB = nodeB.Root.TimerComponent;
            if (sdA == null || sdB == null || timerB == null)
            {
                Log.Console("network partition sdA/sdB/timerB is null");
                return 202;
            }

            int prepareError = await ServiceDiscovery_HA_TestHelper.ConfigureFastLease(sdA, sdB, timerB);
            if (prepareError != 0)
            {
                Log.Console($"network partition fast lease prepare failed: {prepareError}");
                return 203;
            }

            // 3. 模拟旧主网络分区（移除 DB 组件）
            bool isAMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdA, timerB, 5000);
            if (!isAMaster)
            {
                Log.Console("network partition nodeA is not master before partition");
                return 204;
            }

            long epochBefore = sdA.GetOrAddLease().CurrentMasterEpoch;
            nodeA.Root.RemoveComponent<DBManagerComponent>();

            // 4. 验证备机接管且 epoch 增加
            bool bTakeover = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sdB, timerB, 8000);
            if (!bTakeover)
            {
                Log.Console("network partition nodeB takeover timeout");
                return 205;
            }

            long epochAfter = sdB.GetOrAddLease().CurrentMasterEpoch;
            if (epochAfter <= epochBefore)
            {
                Log.Console($"network partition epoch not increased, before: {epochBefore}, after: {epochAfter}");
                return 206;
            }

            // 5. 恢复旧主 DB 后，验证其不再拥有主写权限（fencing 生效）
            nodeA.Root.AddComponent<DBManagerComponent>();

            bool oldMasterAccepted = false;
            try
            {
                oldMasterAccepted = await sdA.EnsureActiveMasterAsync();
            }
            catch (Exception e)
            {
                Log.Debug($"network partition old master ensure throws as expected: {e.Message}");
            }

            if (oldMasterAccepted)
            {
                Log.Console("network partition old master still accepted as master");
                return 207;
            }

            MessageSender sender = nodeB.Root.GetComponent<MessageSender>();
            if (sender == null)
            {
                Log.Console("network partition nodeB sender is null");
                return 208;
            }

            ServiceRegisterRequest request = ServiceRegisterRequest.Create();
            request.SceneName = "PartitionWriteProbe";
            request.ActorId = nodeA.Root.GetActorId();
            request.Metadata[ServiceMetaKey.SceneType] = "PartitionProbe";

            using ServiceRegisterResponse response = await sender.Call(nodeA.Root.GetActorId(), request, false) as ServiceRegisterResponse;
            if (response == null)
            {
                Log.Console("network partition old master call response is null");
                return 209;
            }

            if (response.Error == ErrorCode.ERR_Success)
            {
                Log.Console("network partition fencing failed, old master still accepted write");
                return 210;
            }

            ServiceHeartbeatRequest heartbeatRequest = ServiceHeartbeatRequest.Create();
            heartbeatRequest.AgentActorId = nodeB.Root.GetActorId();
            heartbeatRequest.SceneName = "PartitionHeartbeatProbe";

            using ServiceHeartbeatResponse heartbeatResponse =
                await sender.Call(nodeA.Root.GetActorId(), heartbeatRequest, false) as ServiceHeartbeatResponse;
            if (heartbeatResponse == null)
            {
                Log.Console("network partition old master heartbeat response is null");
                return 213;
            }

            if (heartbeatResponse.Error == ErrorCode.ERR_Success)
            {
                Log.Console("network partition fencing failed, old master still accepted heartbeat");
                return 214;
            }

            Log.Console(
                $"ServiceDiscovery NetworkPartition passed, epochBefore: {epochBefore}, epochAfter: {epochAfter}, oldMasterError: {response.Error}, oldMasterHeartbeatError: {heartbeatResponse.Error}");
            return ErrorCode.ERR_Success;
        }
    }
}
