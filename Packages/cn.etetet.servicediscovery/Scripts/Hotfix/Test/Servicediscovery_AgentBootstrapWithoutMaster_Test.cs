using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// Agent FiberInit 非阻塞语义测试：
    /// 1. Agent Fiber 创建不阻塞等待主节点注册完成。
    /// 2. Proxy 注册到 Agent 不阻塞等待 Agent 向主节点注册成功。
    /// 3. 当主节点出现后，Agent 最终会完成注册并把本地服务同步到主节点。
    /// </summary>
    public class Servicediscovery_AgentFiberInitNonBlocking_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady();
            if (addressError != 0)
            {
                Log.Console($"agent bootstrap ensure address singleton failed: {addressError}");
                return 2;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_AgentFiberInitNonBlocking_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"agent bootstrap reset storage failed: {resetError}");
                return 1;
            }

            int ensureAgentError = await ServiceDiscovery_HA_TestHelper.EnsureServiceDiscoveryAgentFiberAsync(testFiber);
            if (ensureAgentError != 0)
            {
                Log.Console($"agent fiber init nonblocking ensure agent fiber failed: {ensureAgentError}");
                return 6;
            }

            Fiber agentFiber = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryAgentFiber(testFiber);
            ServiceDiscoveryAgent agent = agentFiber.Root.GetComponent<ServiceDiscoveryAgent>();
            // 未创建主节点前，Agent 不应处于 ready（但 Fiber 必须能创建成功）。
            if (agent.IsReady())
            {
                Log.Console("agent fiber init nonblocking agent should not be ready without master");
                return 9;
            }

            // Proxy 注册不应阻塞等待主节点（当前没有主节点仍应立即返回）。
            Fiber proxyFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "NonBlocking_Proxy",
                new StringKV { { "Role", "NonBlocking" } }, waitUntilRegistered: false);
            if (proxyFiber == null)
            {
                Log.Console("agent fiber init nonblocking create proxy fiber failed");
                return 10;
            }

            string proxySceneName = proxyFiber.Root.Name;
            if (!agent.LocalPublishedServices.ContainsKey(proxySceneName))
            {
                Log.Console("agent fiber init nonblocking local published service not recorded in agent");
                return 11;
            }

            Fiber node = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_AgentFiberInitNonBlocking");
            ServiceDiscovery sd = node.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timer = node.Root.TimerComponent;
            bool isMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sd, timer, 5000);
            if (!isMaster)
            {
                Log.Console("agent fiber init nonblocking node is not master");
                return 14;
            }

            bool bound = await ServiceDiscovery_HA_TestHelper.WaitForAgentBoundToMasterAsync(agent, node.Root.GetActorId(), timer, 6000);
            if (!bound)
            {
                Log.Console("agent fiber init nonblocking agent did not bind master after master available");
                return 15;
            }

            bool published = await ServiceDiscovery_HA_TestHelper.WaitForMasterHasService(sd, timer, proxySceneName, 6000);
            if (!published)
            {
                Log.Console("agent fiber init nonblocking proxy service not published to master after master available");
                return 16;
            }

            if (agentFiber.Root.GetComponent<ServiceDiscoveryProxy>() != null)
            {
                Log.Console("agent fiber init nonblocking agent fiber should not mount proxy");
                return 17;
            }

            // 测试成功路径显式控制释放顺序：先释放业务 proxy，再释放 service discovery。
            await testFiber.RemoveFiber(proxyFiber.Id);
            await testFiber.RemoveFiber(node.Id);
            await testFiber.RemoveFiber(agentFiber.Id);

            Log.Console("ServiceDiscovery AgentFiberInitNonBlocking passed");
            return ErrorCode.ERR_Success;
        }
    }

    public class Servicediscovery_AgentFiber_ByZone_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady();
            if (addressError != 0)
            {
                Log.Console($"agent fiber by zone ensure address singleton failed: {addressError}");
                return 2;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_AgentFiber_ByZone_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"agent fiber by zone reset storage failed: {resetError}");
                return 1;
            }

            Fiber node = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_AgentFiber_ByZone");
            ServiceDiscovery sd = node.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timer = node.Root.TimerComponent;
            bool isMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sd, timer, 5000);
            if (!isMaster)
            {
                Log.Console("agent fiber by zone node is not master");
                return 5;
            }

            int ensureAgentError = await ServiceDiscovery_HA_TestHelper.EnsureServiceDiscoveryAgentFiberAsync(testFiber);
            if (ensureAgentError != 0)
            {
                Log.Console($"agent fiber by zone ensure agent failed: {ensureAgentError}");
                return 6;
            }

            Fiber agentFiber = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryAgentFiber(testFiber);
            int expectedAgentFiberId = ServiceDiscoveryFiberHelper.GetAgentFiberId(testFiber.Zone);
            if (agentFiber.Id != expectedAgentFiberId)
            {
                Log.Console($"agent fiber by zone agent id mismatch, expected: {expectedAgentFiberId}, actual: {agentFiber.Id}");
                return 8;
            }

            Fiber proxyFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "AgentFiber_ByZone_Proxy",
                new StringKV { { "Role", "Watcher" } });
            if (proxyFiber == null)
            {
                Log.Console("agent fiber by zone create proxy fiber failed");
                return 9;
            }

            ServiceDiscoveryProxy proxy = proxyFiber.Root.GetComponent<ServiceDiscoveryProxy>();
            if (proxy.AgentFiberInstanceId.Fiber != expectedAgentFiberId)
            {
                Log.Console(
                    $"agent fiber by zone proxy target mismatch, expected: {expectedAgentFiberId}, actual: {proxy.AgentFiberInstanceId.Fiber}");
                return 11;
            }

            return ErrorCode.ERR_Success;
        }
    }
}
