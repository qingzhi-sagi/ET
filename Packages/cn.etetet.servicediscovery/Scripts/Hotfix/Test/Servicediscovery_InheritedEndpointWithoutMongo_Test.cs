using ET.Server;

namespace ET.Test
{
    public class Servicediscovery_InheritedEndpointWithoutMongo_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady(context.Fiber);
            if (addressError != 0)
            {
                Log.Console($"inherited endpoint no mongo ensure address singleton failed: {addressError}");
                return 1;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_InheritedEndpointWithoutMongo_Test));
            Fiber testFiber = scope.TestFiber;
            Fiber serviceDiscoveryFiber = null;
            Fiber agentFiber = null;

            try
            {
                testFiber.AddSingleton<ServiceDiscoveryBootstrapSingleton>();
                serviceDiscoveryFiber = await testFiber.CreateFiber(testFiber.Zone, IdGenerater.Instance.GenerateId(),
                    SceneType.ServiceDiscovery, "ServiceDiscovery_LocalNoMongo");

                if (serviceDiscoveryFiber.Root.GetComponent<DBManagerComponent>() != null)
                {
                    Log.Console("inherited endpoint no mongo service discovery should not add db manager");
                    return 2;
                }

                ServiceDiscovery serviceDiscovery = serviceDiscoveryFiber.Root.GetComponent<ServiceDiscovery>();
                if (serviceDiscovery == null || !serviceDiscovery.GetOrAddLease().IsActiveMaster)
                {
                    Log.Console("inherited endpoint no mongo service discovery should be local master");
                    return 3;
                }

                ActorId masterActorId = serviceDiscoveryFiber.Root.GetActorId();
                if (serviceDiscovery.GetOrAddLease().CurrentMasterActorId != masterActorId)
                {
                    Log.Console("inherited endpoint no mongo master actor id mismatch");
                    return 4;
                }

                ServiceDiscoveryBootstrapSingleton bootstrap = testFiber.GetSingleton<ServiceDiscoveryBootstrapSingleton>();
                if (bootstrap == null || bootstrap.MasterActorId != masterActorId)
                {
                    Log.Console("inherited endpoint no mongo bootstrap master actor id mismatch");
                    return 8;
                }

                long createdAgentId = await testFiber.CreateFiber(SchedulerType.ThreadPool, testFiber.Zone,
                    IdGenerater.Instance.GenerateId(), SceneType.ServiceDiscoveryAgent, "ServiceDiscoveryAgent@NoMongo");
                agentFiber = testFiber.GetFiber(createdAgentId);
                if (agentFiber == null || agentFiber.SchedulerType != SchedulerType.Parent)
                {
                    Log.Console("inherited endpoint no mongo agent should be forced to parent scheduler");
                    return 5;
                }

                if (agentFiber.Root.GetComponent<DBManagerComponent>() != null)
                {
                    Log.Console("inherited endpoint no mongo agent should not add db manager");
                    return 6;
                }

                ServiceDiscoveryAgent agent = agentFiber.Root.GetComponent<ServiceDiscoveryAgent>();
                TimerComponent timer = serviceDiscoveryFiber.Root.TimerComponent;
                bool registered = await ServiceDiscovery_HA_TestHelper.WaitForAgentBoundToMasterAsync(agent, masterActorId, timer, 3000);
                if (!registered)
                {
                    Log.Console("inherited endpoint no mongo agent did not register to inherited master");
                    return 7;
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                testFiber.RemoveSingleton<ServiceDiscoveryBootstrapSingleton>();

                if (agentFiber != null)
                {
                    await testFiber.RemoveFiber(agentFiber.Id);
                }

                if (serviceDiscoveryFiber != null)
                {
                    await testFiber.RemoveFiber(serviceDiscoveryFiber.Id);
                }
            }
        }
    }
}
