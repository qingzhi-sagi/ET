using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// Agent/Proxy 拓扑测试：
    /// 验证单进程多个业务Fiber的Proxy共享同一个Agent Fiber。
    /// </summary>
    public class Servicediscovery_AgentProxyTopology_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady(context.Fiber);
            if (addressError != 0)
            {
                Log.Console($"agent topology ensure address singleton failed: {addressError}");
                return 2;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_AgentProxyTopology_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"agent topology reset storage failed: {resetError}");
                return 1;
            }

            Fiber sdNode = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_AgentTopology");
            ServiceDiscovery sd = sdNode.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timer = sdNode.Root.TimerComponent;
            bool isMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sd, timer, 5000);
            if (!isMaster)
            {
                int forceExpireError = await ServiceDiscovery_HA_TestHelper.ForceMasterLeaseExpireIfNotSelf(sd);
                if (forceExpireError == 0)
                {
                    isMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sd, timer, 5000);
                }

                if (!isMaster)
                {
                    Log.Console("agent topology service discovery node is not master");
                    return 5;
                }
            }

            Fiber proxyFiberA = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "AgentTopology_ProxyA",
                new StringKV { { "Role", "ProxyA" } });
            Fiber proxyFiberB = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "AgentTopology_ProxyB",
                new StringKV { { "Role", "ProxyB" } });
            if (proxyFiberA == null || proxyFiberB == null)
            {
                Log.Console("agent topology create proxy fiber failed");
                return 6;
            }

            ServiceDiscoveryProxy proxyA = proxyFiberA.Root.GetComponent<ServiceDiscoveryProxy>();
            ServiceDiscoveryProxy proxyB = proxyFiberB.Root.GetComponent<ServiceDiscoveryProxy>();
            if (proxyA.AgentFiberInstanceId == default || proxyB.AgentFiberInstanceId == default)
            {
                Log.Console("agent topology proxy agent fiber instance id is empty");
                return 8;
            }

            if (proxyA.AgentFiberInstanceId != proxyB.AgentFiberInstanceId)
            {
                Log.Console(
                    $"agent topology proxies do not share same agent fiber, a: {proxyA.AgentFiberInstanceId}, b: {proxyB.AgentFiberInstanceId}");
                return 9;
            }

            Fiber agentFiber = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryAgentFiber(testFiber);
            if (agentFiber.Root.SceneType != SceneType.ServiceDiscoveryAgent)
            {
                Log.Console(
                    $"agent topology agent scene type mismatch, expected: {SceneType.ServiceDiscoveryAgent}, actual: {agentFiber.Root.SceneType}");
                return 11;
            }

            ServiceDiscoveryAgent agent = agentFiber.Root.GetComponent<ServiceDiscoveryAgent>();
            ServiceDiscoveryProxy agentProxy = agentFiber.Root.GetComponent<ServiceDiscoveryProxy>();
            if (agentProxy != null)
            {
                Log.Console("agent topology agent fiber should not mount ServiceDiscoveryProxy");
                return 13;
            }

            ProcessInnerSender sender = proxyFiberA.Root.GetComponent<ProcessInnerSender>();
            ServiceQueryRequest queryRequest = ServiceQueryRequest.Create();
            queryRequest.Filter["Role"] = "ProxyA";
            using ServiceQueryResponse queryResponse =
                await sender.Call(proxyA.AgentFiberInstanceId, queryRequest, false) as ServiceQueryResponse;
            if (queryResponse == null)
            {
                Log.Console("agent topology query response is null");
                return 15;
            }

            if (queryResponse.Error != ErrorCode.ERR_Success)
            {
                Log.Console($"agent topology query failed: {queryResponse.Error}");
                return 16;
            }

            bool foundProxyA = false;
            foreach (ServiceInfoProto service in queryResponse.Services)
            {
                if (service.SceneName == proxyFiberA.Root.Name)
                {
                    foundProxyA = true;
                    break;
                }
            }

            if (!foundProxyA)
            {
                Log.Console("agent topology query not found proxyA service");
                return 17;
            }

            // 测试成功路径显式控制释放顺序：先释放业务 proxy，再释放 service discovery。
            await testFiber.RemoveFiber(proxyFiberA.Id);
            await testFiber.RemoveFiber(proxyFiberB.Id);
            await testFiber.RemoveFiber(sdNode.Id);

            Log.Console($"ServiceDiscovery AgentProxyTopology passed, agent: {proxyA.AgentFiberInstanceId}");
            return ErrorCode.ERR_Success;
        }
    }
}
