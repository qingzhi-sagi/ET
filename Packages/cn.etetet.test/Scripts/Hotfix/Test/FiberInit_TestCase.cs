using ET.Server;

namespace ET.Test
{
    /// <summary>
    /// 每个测试用例都是一个全新的环境，全新的服务器环境
    /// </summary>
    [Invoke(SceneType.TestCase)]
    public class FiberInit_TestCase: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            LogMsg.Instance.AddIgnore(typeof(ServiceHeartbeatRequest));
            LogMsg.Instance.AddIgnore(typeof(ServiceHeartbeatResponse));
            
            Fiber fiber = fiberInit.Fiber;
            Scene root = fiber.Root;
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<DBManagerComponent>();

            TestFiberDatabaseCleanupComponent cleanupComponent = root.AddComponent<TestFiberDatabaseCleanupComponent>();
            cleanupComponent.RegisterLogicalDbName(ServiceDiscoveryPersistenceConst.DBName);
            await cleanupComponent.CleanupAsync("FiberInit_TestCase");
            
            int process = Options.Instance.Process;
            
            StartProcessConfig startProcessConfig = fiber.GetSingleton<StartProcessConfigCategory>().Get(process);
            
            World.Instance.AddSingleton<AddressSingleton>();
            // 先看环境变量是否有地址传过来，如果没有，则使用StartProcessConfig的地址跟端口
            AddressHelper.SetInnerIPInnerPortOuterIP(fiber, startProcessConfig);

            await fiber.CreateFiber(IdGenerater.Instance.GenerateId(), SceneType.ServiceDiscovery, nameof(SceneType.ServiceDiscovery));

            // 进程级ServiceDiscovery Agent Fiber：所有业务Fiber的ServiceDiscoveryProxy统一通过此Fiber转发。
            await fiber.CreateFiberWithId(Const.ServiceDiscoveryAgentFiberId, SchedulerType.ThreadPool, IdGenerater.Instance.GenerateId(),
                SceneType.ServiceDiscoveryAgent, $"ServiceDiscoveryAgent@{process}@{Options.Instance.ReplicaIndex}");

            // 根据配置创建纤程
            var scenes = fiber.GetSingleton<StartSceneConfigCategory>().GetByProcess(process);
            
            foreach (StartSceneConfig startConfig in scenes)
            {
                int sceneType = SceneTypeSingleton.Instance.GetSceneType(startConfig.SceneType);
                if (sceneType == SceneType.ServiceDiscovery)
                {
                    continue;
                }

                await fiber.CreateFiber(startConfig.Id, sceneType, startConfig.Name);
            }
        }
    }

    [Event(SceneType.All)]
    public class FiberDestroyEvent_TestFiberDatabaseCleanup : AEvent<Scene, FiberDestroyEvent>
    {
        protected override async ETTask Run(Scene scene, FiberDestroyEvent args)
        {
            TestFiberDatabaseCleanupComponent cleanupComponent = scene.GetComponent<TestFiberDatabaseCleanupComponent>();
            if (cleanupComponent == null)
            {
                return;
            }

            await cleanupComponent.CleanupAsync(nameof(FiberDestroyEvent));
        }
    }
}
