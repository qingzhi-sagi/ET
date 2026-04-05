using System.Collections.Generic;
using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 服务查询与索引测试：
    /// 验证 Agent Query RPC、索引命中和多值查询结果一致性。
    /// </summary>
    public class Servicediscovery_QueryAndIndex_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            // 1. 准备测试环境

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady();
            if (addressError != 0)
            {
                Log.Console($"query ensure address singleton failed: {addressError}");
                return 2;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_QueryAndIndex_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"query reset storage failed: {resetError}");
                return 1;
            }

            Fiber node = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0, "ServiceDiscovery_Query");
            if (node == null)
            {
                Log.Console("query create service discovery node failed");
                return 3;
            }

            ServiceDiscovery sd = node.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timer = node.Root.TimerComponent;
            if (sd == null || timer == null)
            {
                Log.Console("query sd or timer is null");
                return 4;
            }

            bool isMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sd, timer, 5000);
            if (!isMaster)
            {
                Log.Console("query node is not master");
                return 5;
            }

            int ensureAgentError = await ServiceDiscovery_HA_TestHelper.EnsureServiceDiscoveryAgentFiberAsync(testFiber);
            if (ensureAgentError != 0)
            {
                Log.Console($"query ensure service discovery agent failed: {ensureAgentError}");
                return 6;
            }

            // 2. 注册三类测试服务（Gate*2 + Realm*1）
            Address address = node.Root.GetActorId().Address;
            await sd.RegisterServiceAsync("QueryGate_1", ServiceDiscovery_HA_TestHelper.CreateActorId(testFiber, address, 810001), new StringKV
            {
                { ServiceMetaKey.SceneType, "Gate" },
                { "Group", "A" },
            });
            await sd.RegisterServiceAsync("QueryGate_2", ServiceDiscovery_HA_TestHelper.CreateActorId(testFiber, address, 810002), new StringKV
            {
                { ServiceMetaKey.SceneType, "Gate" },
                { "Group", "B" },
            });
            await sd.RegisterServiceAsync("QueryRealm_1", ServiceDiscovery_HA_TestHelper.CreateActorId(testFiber, address, 810003), new StringKV
            {
                { ServiceMetaKey.SceneType, "Realm" },
                { "Group", "B" },
            });

            // 3. 验证仅通过 Agent 查询，结果与预期一致
            MessageSender sender = node.Root.GetComponent<MessageSender>();
            if (sender == null)
            {
                Log.Console("query message sender is null");
                return 9;
            }

            ActorId agentActorId = new ActorId(node.Root.GetActorId().Address,
                ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryAgentFiberInstanceId(testFiber.Zone));

            bool gateMatched = await WaitForAgentQueryCount(sender, agentActorId, timer, new StringKV
            {
                { ServiceMetaKey.SceneType, "Gate" },
            }, 2, 6000);
            if (!gateMatched)
            {
                Log.Console("query gate filter count invalid via agent");
                return 10;
            }

            bool multiTypeMatched = await WaitForAgentQueryCount(sender, agentActorId, timer, new StringKV
            {
                { ServiceMetaKey.SceneType, "Gate,Realm" },
            }, 3, 6000);
            if (!multiTypeMatched)
            {
                Log.Console("query multi type filter count invalid via agent");
                return 11;
            }

            List<ServiceInfoProto> gateGroupServices = await QueryServicesByFilter(sender, agentActorId, new StringKV
            {
                { ServiceMetaKey.SceneType, "Gate" },
                { "Group", "A" },
            });
            if (gateGroupServices.Count != 1 || gateGroupServices[0].SceneName != "QueryGate_1")
            {
                Log.Console(
                    $"query gate group filter invalid count/name via agent: {gateGroupServices.Count}/{(gateGroupServices.Count > 0 ? gateGroupServices[0].SceneName : "none")}");
                return 12;
            }

            Log.Console("ServiceDiscovery QueryAndIndex passed");
            return ErrorCode.ERR_Success;
        }

        private static async ETTask<bool> WaitForAgentQueryCount(MessageSender sender, ActorId agentActorId, TimerComponent timer,
            StringKV filter, int expectedCount, long timeoutMs)
        {
            EntityRef<TimerComponent> timerRef = timer;
            long deadline = TimeInfo.Instance.ServerNow() + timeoutMs;
            while (TimeInfo.Instance.ServerNow() <= deadline)
            {
                List<ServiceInfoProto> services = await QueryServicesByFilter(sender, agentActorId, filter);
                if (services.Count == expectedCount)
                {
                    return true;
                }

                timer = timerRef;
                if (timer == null)
                {
                    return false;
                }

                await timer.WaitAsync(100);
            }

            return false;
        }

        private static async ETTask<List<ServiceInfoProto>> QueryServicesByFilter(MessageSender sender, ActorId agentActorId, StringKV filter)
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
                return new List<ServiceInfoProto>();
            }

            List<ServiceInfoProto> services = new(response.Services.Count);
            foreach (ServiceInfoProto serviceProto in response.Services)
            {
                if (serviceProto == null || string.IsNullOrEmpty(serviceProto.SceneName) || serviceProto.ActorId == default)
                {
                    continue;
                }

                services.Add(serviceProto);
            }

            return services;
        }
    }
}
