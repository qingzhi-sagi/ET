using System;
using System.Collections.Generic;
using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 多客户端并发压力测试：
    /// 并发注册/注销大量服务，验证 Agent 查询结果、订阅端状态一致性与性能。
    /// </summary>
    public class Servicediscovery_MultiClientConcurrentPressure_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            // 1. 准备测试环境

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady(context.Fiber);
            if (addressError != 0)
            {
                Log.Console($"pressure ensure address singleton failed: {addressError}");
                return 2;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_MultiClientConcurrentPressure_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"pressure reset storage failed: {resetError}");
                return 1;
            }

            Fiber node = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_Pressure");
            ServiceDiscovery sd = node.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timer = node.Root.TimerComponent;
            MessageSender sender = node.Root.GetComponent<MessageSender>();
            bool isMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sd, timer, 5000);
            if (!isMaster)
            {
                Log.Console("pressure node is not master");
                return 5;
            }

            Fiber watcherFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "Pressure_Watcher",
                new StringKV { { "Role", "Watcher" } });
            if (watcherFiber == null)
            {
                Log.Console("pressure create watcher fiber failed");
                return 6;
            }

            ServiceDiscoveryProxy watcherProxy = watcherFiber.Root.GetComponent<ServiceDiscoveryProxy>();
            TimerComponent watcherTimer = watcherFiber.Root.TimerComponent;
            // 2. 并发注册压力：300 个 provider 同时注册
            StringKV pressureFilter = new StringKV { { "Role", "PressureProvider" } };
            await watcherProxy.SubscribeServiceChange("PressureFilter", pressureFilter);

            ActorId masterActorId = node.Root.GetActorId();
            ActorId agentActorId = new ActorId(masterActorId.Address,
                ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryAgentFiberInstanceId(testFiber));
            Address address = masterActorId.Address;
            const int clientCount = 300;
            const int removeCount = 120;

            long registerBegin = testFiber.GetSingleton<TimeInfo>().ServerNow();
            List<ETTask> registerTasks = new(clientCount);
            for (int i = 0; i < clientCount; ++i)
            {
                string sceneName = $"PressureProvider_{i:D4}";
                ActorId actorId = ServiceDiscovery_HA_TestHelper.CreateActorId(testFiber, address, 830000 + i);
                registerTasks.Add(RegisterService(sender, masterActorId, sceneName, actorId));
            }

            await ETTask.WaitAll(registerTasks);
            long registerCost = testFiber.GetSingleton<TimeInfo>().ServerNow() - registerBegin;
            if (registerCost > 15000)
            {
                Log.Console($"pressure register cost too high: {registerCost}ms");
                return 8;
            }

            bool agentSyncedAll = await WaitForAgentFilterCount(sender, agentActorId, timer, pressureFilter, clientCount, 8000);
            if (!agentSyncedAll)
            {
                Log.Console($"pressure agent count did not settle to expected total: {clientCount}");
                return 9;
            }

            bool watcherSyncedAll = await WaitForProxyFilterCount(watcherProxy, watcherTimer, pressureFilter, clientCount, 8000);
            if (!watcherSyncedAll)
            {
                Log.Console("pressure watcher cache did not sync to full count");
                return 10;
            }

            // 3. 并发注销压力：批量删除前 120 个服务
            long unregisterBegin = testFiber.GetSingleton<TimeInfo>().ServerNow();
            List<ETTask> unregisterTasks = new(removeCount);
            for (int i = 0; i < removeCount; ++i)
            {
                string sceneName = $"PressureProvider_{i:D4}";
                unregisterTasks.Add(UnregisterService(sender, masterActorId, sceneName));
            }

            await ETTask.WaitAll(unregisterTasks);
            long unregisterCost = testFiber.GetSingleton<TimeInfo>().ServerNow() - unregisterBegin;
            if (unregisterCost > 10000)
            {
                Log.Console($"pressure unregister cost too high: {unregisterCost}ms");
                return 11;
            }

            int expectedRemain = clientCount - removeCount;
            bool agentSettled = await WaitForAgentFilterCount(sender, agentActorId, timer, pressureFilter, expectedRemain, 8000);
            if (!agentSettled)
            {
                Log.Console($"pressure agent count not settled to expected remain: {expectedRemain}");
                return 12;
            }

            bool watcherSettled = await WaitForProxyFilterCount(watcherProxy, watcherTimer, pressureFilter, expectedRemain, 8000);
            if (!watcherSettled)
            {
                Log.Console($"pressure watcher count not settled to expected remain: {expectedRemain}");
                return 13;
            }

            // 测试成功路径显式控制释放顺序：先释放业务 proxy，再释放 service discovery。
            await testFiber.RemoveFiber(watcherFiber.Id);
            await testFiber.RemoveFiber(node.Id);

            Log.Console(
                $"ServiceDiscovery MultiClientConcurrentPressure passed, registerCost: {registerCost}ms, unregisterCost: {unregisterCost}ms, total: {clientCount}");
            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 单个并发注册任务。
        /// </summary>
        private static async ETTask RegisterService(MessageSender sender, ActorId masterActorId, string sceneName, ActorId actorId)
        {
            ServiceRegisterRequest request = ServiceRegisterRequest.Create();
            request.SceneName = sceneName;
            request.ActorId = actorId;
            request.Metadata[ServiceMetaKey.SceneType] = "PressureProvider";
            request.Metadata["Role"] = "PressureProvider";

            using ServiceRegisterResponse response = await sender.Call(masterActorId, request, false) as ServiceRegisterResponse;
            if (response == null)
            {
                throw new Exception($"pressure register response null: {sceneName}");
            }

            if (response.Error != ErrorCode.ERR_Success)
            {
                throw new Exception($"pressure register error: {sceneName} error: {response.Error}");
            }
        }

        /// <summary>
        /// 单个并发注销任务。
        /// </summary>
        private static async ETTask UnregisterService(MessageSender sender, ActorId masterActorId, string sceneName)
        {
            ServiceUnregisterRequest request = ServiceUnregisterRequest.Create();
            request.SceneName = sceneName;

            using ServiceUnregisterResponse response = await sender.Call(masterActorId, request, false) as ServiceUnregisterResponse;
            if (response == null)
            {
                throw new Exception($"pressure unregister response null: {sceneName}");
            }

            if (response.Error != ErrorCode.ERR_Success)
            {
                throw new Exception($"pressure unregister error: {sceneName} error: {response.Error}");
            }
        }

        /// <summary>
        /// 等待 Agent 查询按过滤条件收敛到预期数量。
        /// </summary>
        private static async ETTask<bool> WaitForAgentFilterCount(MessageSender sender, ActorId agentActorId, TimerComponent timer,
            StringKV filter, int expected,
            long timeoutMs)
        {
            EntityRef<TimerComponent> timerRef = timer;
            long deadline = timer.GetSingleton<TimeInfo>().ServerNow() + timeoutMs;
            while (timer.GetSingleton<TimeInfo>().ServerNow() <= deadline)
            {
                int count = await QueryAgentFilterCount(sender, agentActorId, filter);
                if (count == expected)
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

        private static async ETTask<int> QueryAgentFilterCount(MessageSender sender, ActorId agentActorId, StringKV filter)
        {
            using ServiceQueryRequest request = ServiceQueryRequest.Create();
            if (filter != null)
            {
                foreach ((string key, string value) in filter)
                {
                    request.Filter[key] = value;
                }
            }

            using ServiceQueryResponse response = await sender.Call(agentActorId, request, false) as ServiceQueryResponse;
            if (response == null || response.Error != ErrorCode.ERR_Success)
            {
                return -1;
            }

            return response.Services.Count;
        }

        /// <summary>
        /// 等待 Proxy 本地缓存按过滤条件收敛到预期数量。
        /// </summary>
        private static async ETTask<bool> WaitForProxyFilterCount(ServiceDiscoveryProxy proxy, TimerComponent timer, StringKV filter,
            int expected, long timeoutMs)
        {
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

                int count = proxy.GetServiceInfoByFilter(filter).Count;
                if (count == expected)
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
