using System.Reflection;
using ET.Server;

namespace ET.Test
{
    [MessageHandler(SceneType.TestEmpty)]
    public class ServiceDiscoveryTest_ServiceAgentRegisterRequestHandler :
        MessageHandler<Scene, ServiceAgentRegisterRequest, ServiceAgentRegisterResponse>
    {
        protected override async ETTask Run(Scene scene, ServiceAgentRegisterRequest request, ServiceAgentRegisterResponse response)
        {
            ServiceDiscoveryTestSlowHeartbeatEndpoint endpoint = scene.GetComponent<ServiceDiscoveryTestSlowHeartbeatEndpoint>();
            if (endpoint != null)
            {
                ++endpoint.RegisterRequestCount;
            }

            await ETTask.CompletedTask;
        }
    }

    [MessageHandler(SceneType.TestEmpty)]
    public class ServiceDiscoveryTest_ServiceHeartbeatRequestHandler :
        MessageHandler<Scene, ServiceHeartbeatRequest, ServiceHeartbeatResponse>
    {
        protected override async ETTask Run(Scene scene, ServiceHeartbeatRequest request, ServiceHeartbeatResponse response)
        {
            ServiceDiscoveryTestSlowHeartbeatEndpoint endpoint = scene.GetComponent<ServiceDiscoveryTestSlowHeartbeatEndpoint>();
            if (endpoint == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            ++endpoint.HeartbeatRequestCount;
            ++endpoint.CurrentHeartbeatRequests;
            if (endpoint.CurrentHeartbeatRequests > endpoint.MaxConcurrentHeartbeatRequests)
            {
                endpoint.MaxConcurrentHeartbeatRequests = endpoint.CurrentHeartbeatRequests;
            }

            EntityRef<ServiceDiscoveryTestSlowHeartbeatEndpoint> endpointRef = endpoint;
            await scene.Root().TimerComponent.WaitAsync(endpoint.HeartbeatDelayMs);
            endpoint = endpointRef;
            if (endpoint != null)
            {
                --endpoint.CurrentHeartbeatRequests;
            }
        }
    }

    [SkipAwaitEntityCheck]
    public class Servicediscovery_AgentHeartbeatNoBacklog_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady(context.Fiber);
            if (addressError != 0)
            {
                Log.Console($"agent heartbeat backlog ensure address singleton failed: {addressError}");
                return 802;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_AgentHeartbeatNoBacklog_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"agent heartbeat backlog reset storage failed: {resetError}");
                return 801;
            }

            Fiber slowMasterFiber = await testFiber.CreateFiber(IdGenerater.Instance.GenerateId(),
                SceneType.TestEmpty, "ServiceDiscovery_SlowHeartbeatMaster");
            if (slowMasterFiber == null)
            {
                Log.Console("agent heartbeat backlog create slow master fiber failed");
                return 803;
            }

            Scene slowMasterRoot = slowMasterFiber.Root;
            slowMasterRoot.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            slowMasterRoot.AddComponent<TimerComponent>();
            slowMasterRoot.AddComponent<CoroutineLockComponent>();
            slowMasterRoot.AddComponent<ProcessInnerSender>();
            slowMasterRoot.AddComponent<MessageSender>();
            ServiceDiscoveryTestSlowHeartbeatEndpoint endpoint = slowMasterRoot.AddComponent<ServiceDiscoveryTestSlowHeartbeatEndpoint>();
            endpoint.HeartbeatDelayMs = 8 * 1000;

            DBManagerComponent dbManager = testFiber.Root.GetComponent<DBManagerComponent>();
            DBComponent db = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryDB(dbManager);
            if (db == null)
            {
                Log.Console("agent heartbeat backlog db is null");
                return 804;
            }

            long now = testFiber.GetSingleton<TimeInfo>().ServerNow();
            await ServiceDiscovery_HA_TestHelper.SaveMasterRecordAsync(testFiber.Root, db, slowMasterRoot.Name,
                slowMasterRoot.GetActorId(), 1, now + 10 * 1000, 10 * 1000, now);

            int ensureAgentError = await ServiceDiscovery_HA_TestHelper.EnsureServiceDiscoveryAgentFiberAsync(testFiber);
            if (ensureAgentError != 0)
            {
                Log.Console($"agent heartbeat backlog ensure agent fiber failed: {ensureAgentError}");
                return 805;
            }

            Fiber agentFiber = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryAgentFiber(testFiber);
            ServiceDiscoveryAgent agent = agentFiber?.Root?.GetComponent<ServiceDiscoveryAgent>();
            TimerComponent agentTimer = agentFiber?.Root?.TimerComponent;
            if (agent == null || agentTimer == null)
            {
                Log.Console("agent heartbeat backlog agent or timer is null");
                return 806;
            }

            bool bound = await ServiceDiscovery_HA_TestHelper.WaitForAgentBoundToMasterAsync(
                agent, slowMasterRoot.GetActorId(), agentTimer, 5000);
            if (!bound)
            {
                Log.Console("agent heartbeat backlog agent did not bind slow master");
                return 807;
            }

            ServiceDiscoveryAgentMasterHeartbeat heartbeat = agent.GetComponent<ServiceDiscoveryAgentMasterHeartbeat>();
            if (heartbeat == null)
            {
                Log.Console("agent heartbeat backlog heartbeat component is null");
                return 808;
            }

            endpoint.RegisterRequestCount = 0;
            endpoint.HeartbeatRequestCount = 0;
            endpoint.CurrentHeartbeatRequests = 0;
            endpoint.MaxConcurrentHeartbeatRequests = 0;

            await agentTimer.WaitAsync(4500);

            if (endpoint.HeartbeatRequestCount <= 0)
            {
                Log.Console("agent heartbeat backlog expected heartbeat loop to send at least one request");
                return 810;
            }

            if (endpoint.MaxConcurrentHeartbeatRequests > 1)
            {
                Log.Console($"agent heartbeat backlog concurrent heartbeat count invalid: {endpoint.MaxConcurrentHeartbeatRequests}");
                return 811;
            }

            Log.Console("ServiceDiscovery AgentHeartbeatNoBacklog passed");
            return ErrorCode.ERR_Success;
        }
    }

    [SkipAwaitEntityCheck]
    public class Servicediscovery_AgentHeartbeatRpcTimeout_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady(context.Fiber);
            if (addressError != 0)
            {
                Log.Console($"agent heartbeat rpc timeout ensure address singleton failed: {addressError}");
                return 822;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_AgentHeartbeatRpcTimeout_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"agent heartbeat rpc timeout reset storage failed: {resetError}");
                return 821;
            }

            Fiber slowMasterFiber = await testFiber.CreateFiber(IdGenerater.Instance.GenerateId(),
                SceneType.TestEmpty, "ServiceDiscovery_SlowHeartbeatTimeoutMaster");
            if (slowMasterFiber == null)
            {
                Log.Console("agent heartbeat rpc timeout create slow master fiber failed");
                return 823;
            }

            Scene slowMasterRoot = slowMasterFiber.Root;
            slowMasterRoot.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            slowMasterRoot.AddComponent<TimerComponent>();
            slowMasterRoot.AddComponent<CoroutineLockComponent>();
            slowMasterRoot.AddComponent<ProcessInnerSender>();
            slowMasterRoot.AddComponent<MessageSender>();
            ServiceDiscoveryTestSlowHeartbeatEndpoint endpoint = slowMasterRoot.AddComponent<ServiceDiscoveryTestSlowHeartbeatEndpoint>();
            endpoint.HeartbeatDelayMs = 8 * 1000;

            DBManagerComponent dbManager = testFiber.Root.GetComponent<DBManagerComponent>();
            DBComponent db = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryDB(dbManager);
            if (db == null)
            {
                Log.Console("agent heartbeat rpc timeout db is null");
                return 824;
            }

            long now = testFiber.GetSingleton<TimeInfo>().ServerNow();
            await ServiceDiscovery_HA_TestHelper.SaveMasterRecordAsync(testFiber.Root, db, slowMasterRoot.Name,
                slowMasterRoot.GetActorId(), 1, now + 10 * 1000, 10 * 1000, now);

            int ensureAgentError = await ServiceDiscovery_HA_TestHelper.EnsureServiceDiscoveryAgentFiberAsync(testFiber);
            if (ensureAgentError != 0)
            {
                Log.Console($"agent heartbeat rpc timeout ensure agent fiber failed: {ensureAgentError}");
                return 825;
            }

            Fiber agentFiber = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryAgentFiber(testFiber);
            ServiceDiscoveryAgent agent = agentFiber?.Root?.GetComponent<ServiceDiscoveryAgent>();
            TimerComponent agentTimer = agentFiber?.Root?.TimerComponent;
            if (agent == null || agentTimer == null)
            {
                Log.Console("agent heartbeat rpc timeout agent or timer is null");
                return 826;
            }

            bool bound = await ServiceDiscovery_HA_TestHelper.WaitForAgentBoundToMasterAsync(
                agent, slowMasterRoot.GetActorId(), agentTimer, 5000);
            if (!bound)
            {
                Log.Console("agent heartbeat rpc timeout agent did not bind slow master");
                return 827;
            }

            ServiceDiscoveryAgentMasterHeartbeat heartbeat = agent.GetComponent<ServiceDiscoveryAgentMasterHeartbeat>();
            if (heartbeat == null)
            {
                Log.Console("agent heartbeat rpc timeout heartbeat component is null");
                return 828;
            }

            heartbeat.RpcTimeout = 5 * 1000;
            MethodInfo sendMethod = typeof(ServiceDiscoveryAgentMasterHeartbeatSystem).GetMethod(
                "SendHeartbeatToDiscoveryAsync", BindingFlags.Static | BindingFlags.NonPublic);
            if (sendMethod == null)
            {
                Log.Console("agent heartbeat rpc timeout reflection method missing");
                return 829;
            }

            endpoint.RegisterRequestCount = 0;

            long beginTime = testFiber.GetSingleton<TimeInfo>().ServerNow();
            ETTask heartbeatTask = sendMethod.Invoke(null, new object[] { heartbeat }) as ETTask;
            if (heartbeatTask == null)
            {
                Log.Console("agent heartbeat rpc timeout reflection invoke returned null");
                return 830;
            }

            await heartbeatTask;
            long costTime = testFiber.GetSingleton<TimeInfo>().ServerNow() - beginTime;

            if (costTime >= 7 * 1000)
            {
                Log.Console($"agent heartbeat rpc timeout cost too long: {costTime}ms");
                return 831;
            }

            bool rebound = await ServiceDiscovery_HA_TestHelper.WaitForAgentBoundToMasterAsync(
                agent, slowMasterRoot.GetActorId(), agentTimer, 3000);
            if (!rebound)
            {
                Log.Console("agent heartbeat rpc timeout agent did not rebind after timeout failover");
                return 832;
            }

            if (endpoint.RegisterRequestCount <= 0)
            {
                Log.Console("agent heartbeat rpc timeout did not trigger re-register");
                return 833;
            }

            Log.Console($"ServiceDiscovery AgentHeartbeatRpcTimeout passed, cost: {costTime}ms");
            return ErrorCode.ERR_Success;
        }
    }
}
