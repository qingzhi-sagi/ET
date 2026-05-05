using ET.Server;

namespace ET.Test
{
    public class Test_TestCaseServiceDiscoveryWithoutMongo_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestCase,
                nameof(Test_TestCaseServiceDiscoveryWithoutMongo_Test));
            Fiber testFiber = scope.TestFiber;

            Fiber serviceDiscoveryFiber = testFiber.GetFiber(nameof(SceneType.ServiceDiscovery));
            if (serviceDiscoveryFiber == null)
            {
                Log.Console("test case service discovery fiber not found");
                return 1;
            }

            if (serviceDiscoveryFiber.Root.GetComponent<DBManagerComponent>() != null)
            {
                Log.Console("test case service discovery should not add db manager");
                return 2;
            }

            ServiceDiscovery serviceDiscovery = serviceDiscoveryFiber.Root.GetComponent<ServiceDiscovery>();
            ActorId masterActorId = serviceDiscoveryFiber.Root.GetActorId();
            if (serviceDiscovery == null ||
                !serviceDiscovery.GetOrAddLease().IsActiveMaster ||
                serviceDiscovery.GetOrAddLease().CurrentMasterActorId != masterActorId)
            {
                Log.Console("test case service discovery should be local active master");
                return 3;
            }

            Fiber agentFiber = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryAgentFiber(testFiber);
            if (agentFiber == null)
            {
                Log.Console("test case service discovery agent fiber not found");
                return 4;
            }

            if (agentFiber.Root.GetComponent<DBManagerComponent>() != null)
            {
                Log.Console("test case service discovery agent should not add db manager");
                return 5;
            }

            ServiceDiscoveryAgent agent = agentFiber.Root.GetComponent<ServiceDiscoveryAgent>();
            TimerComponent timer = serviceDiscoveryFiber.Root.TimerComponent;
            bool registered = await ServiceDiscovery_HA_TestHelper.WaitForAgentBoundToMasterAsync(agent, masterActorId, timer, 3000);
            if (!registered)
            {
                Log.Console("test case service discovery agent did not register to inherited master");
                return 6;
            }

            return ErrorCode.ERR_Success;
        }
    }
}
