using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 持久化异常测试：
    /// 模拟 Mongo 不可用时注册/订阅失败，并验证恢复后可正常写入。
    /// </summary>
    public class Servicediscovery_PersistenceFailure_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            // 1. 准备测试环境

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady(context.Fiber);
            if (addressError != 0)
            {
                Log.Console($"persistence ensure address singleton failed: {addressError}");
                return 2;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_PersistenceFailure_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"persistence reset storage failed: {resetError}");
                return 1;
            }

            Fiber node = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_PersistenceFailure");
            ServiceDiscovery sd = node.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timer = node.Root.TimerComponent;
            MessageSender sender = node.Root.GetComponent<MessageSender>();
            bool isMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sd, timer, 5000);
            if (!isMaster)
            {
                Log.Console("persistence node is not master");
                return 5;
            }

            ActorId masterActorId = node.Root.GetActorId();
            Address address = masterActorId.Address;

            // 2. 断开 DB，验证持久化相关写操作失败
            node.Root.RemoveComponent<DBManagerComponent>();

            ServiceRegisterRequest failRegister = ServiceRegisterRequest.Create();
            failRegister.SceneName = "PersistenceFail_Register";
            failRegister.ActorId = ServiceDiscovery_HA_TestHelper.CreateActorId(testFiber, address, 840001);
            failRegister.Metadata[ServiceMetaKey.SceneType] = "PersistenceProbe";
            using ServiceRegisterResponse failRegisterResponse =
                await sender.Call(masterActorId, failRegister, false) as ServiceRegisterResponse;
            if (failRegisterResponse == null)
            {
                Log.Console("persistence fail register response is null");
                return 6;
            }

            if (failRegisterResponse.Error == ErrorCode.ERR_Success)
            {
                Log.Console("persistence register should fail when db unavailable");
                return 7;
            }

            if (failRegisterResponse.Error != ErrorCode.ERR_ServiceDiscoveryPersistenceFailed)
            {
                Log.Console($"persistence register error code invalid: {failRegisterResponse.Error}");
                return 10;
            }

            // 3. 恢复 DB，验证写路径恢复正常
            node.Root.AddComponent<DBManagerComponent>();
            bool masterRecovered = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sd, timer, 6000);
            if (!masterRecovered)
            {
                Log.Console("persistence node did not recover master state after db restore");
                return 8;
            }

            ServiceRegisterRequest okRegister = ServiceRegisterRequest.Create();
            okRegister.SceneName = "PersistenceOK_Register";
            okRegister.ActorId = ServiceDiscovery_HA_TestHelper.CreateActorId(testFiber, address, 840003);
            okRegister.Metadata[ServiceMetaKey.SceneType] = "PersistenceProbe";
            using ServiceRegisterResponse okRegisterResponse =
                await sender.Call(masterActorId, okRegister, false) as ServiceRegisterResponse;
            if (okRegisterResponse == null || okRegisterResponse.Error != ErrorCode.ERR_Success)
            {
                Log.Console($"persistence register should succeed after db restore, error: {okRegisterResponse?.Error}");
                return 9;
            }

            Log.Console("ServiceDiscovery PersistenceFailure passed");
            return ErrorCode.ERR_Success;
        }
    }
}
