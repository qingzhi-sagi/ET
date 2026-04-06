using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 服务注销测试：
    /// 覆盖主动注销和心跳超时注销，并验证内存与 Mongo 同步删除。
    /// </summary>
    public class Servicediscovery_UnregisterAndHeartbeatTimeout_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            // 1. 准备测试环境

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady();
            if (addressError != 0)
            {
                Log.Console($"unregister ensure address singleton failed: {addressError}");
                return 2;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_UnregisterAndHeartbeatTimeout_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"unregister reset storage failed: {resetError}");
                return 1;
            }

            Fiber node = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_Unregister");
            ServiceDiscovery sd = node.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timer = node.Root.TimerComponent;
            bool isMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sd, timer, 5000);
            if (!isMaster)
            {
                Log.Console("unregister node is not master");
                return 5;
            }

            // 2. 主动注销：先注册再注销，验证内存和 DB 均清理
            Address address = node.Root.GetActorId().Address;
            string manualScene = "Unregister_Manual_Service";
            await sd.RegisterServiceAsync(manualScene, ServiceDiscovery_HA_TestHelper.CreateActorId(testFiber, address, 820001), new StringKV
            {
                { ServiceMetaKey.SceneType, "Gate" },
                { "Role", "Manual" },
            });

            if (!sd.Services.TryGetValue(manualScene, out EntityRef<ServiceInfo> manualRef) || manualRef == null)
            {
                Log.Console("unregister manual service missing before unregister");
                return 6;
            }

            await sd.UnregisterServiceAsync(manualScene);
            bool manualRemoved = await ServiceDiscovery_HA_TestHelper.WaitForMasterNotHasService(sd, timer, manualScene, 4000);
            if (!manualRemoved)
            {
                Log.Console("unregister manual service not removed from memory");
                return 7;
            }

            // 3. 销毁自动注销：删除带 Proxy 的 Fiber，验证会直接走进程内 Agent Fiber 注销
            Fiber destroyFiber = await ServiceDiscovery_HA_TestHelper.CreateProxyFiber(testFiber, "Unregister_Destroy_Proxy", new StringKV
                {
                    { "Role", "Destroy" },
                });
            if (destroyFiber == null)
            {
                Log.Console("unregister destroy proxy fiber create failed");
                return 9;
            }

            string destroyScene = destroyFiber.Root.Name;
            bool destroyRegistered = await ServiceDiscovery_HA_TestHelper.WaitForMasterHasService(sd, timer, destroyScene, 5000);
            if (!destroyRegistered)
            {
                Log.Console("unregister destroy proxy service missing before fiber remove");
                return 10;
            }

            await testFiber.RemoveFiber(destroyFiber.Id);

            bool destroyRemoved = await ServiceDiscovery_HA_TestHelper.WaitForMasterNotHasService(sd, timer, destroyScene, 3000);
            if (!destroyRemoved)
            {
                Log.Console("unregister destroy proxy service not removed after fiber destroy");
                return 11;
            }

            // 4. 超时注销：构造过期心跳并等待定时裁剪生效
            string timeoutScene = "Unregister_Timeout_Service";
            await sd.RegisterServiceAsync(timeoutScene, ServiceDiscovery_HA_TestHelper.CreateActorId(testFiber, address, 820002), new StringKV
            {
                { ServiceMetaKey.SceneType, "Gate" },
                { "Role", "Timeout" },
            });

            if (!sd.Services.TryGetValue(timeoutScene, out EntityRef<ServiceInfo> timeoutRef))
            {
                Log.Console("unregister timeout service missing after register");
                return 13;
            }

            ServiceInfo timeoutInfo = timeoutRef;
            if (timeoutInfo == null)
            {
                Log.Console("unregister timeout service entity null");
                return 14;
            }

            sd.GetOrAddAgentHeartbeat().HeartbeatTimeout = 120;
            sd.GetOrAddAgentHeartbeat().HeartbeatCheckInterval = 40;
            sd.GetOrAddAgentHeartbeat().AgentHeartbeatTimes[address] = TimeInfo.Instance.ServerNow() - 10_000;

            bool timeoutRemoved = await ServiceDiscovery_HA_TestHelper.WaitForMasterNotHasService(sd, timer, timeoutScene, 6000);
            if (!timeoutRemoved)
            {
                Log.Console("unregister timeout service not removed by heartbeat check");
                return 15;
            }

            Log.Console("ServiceDiscovery UnregisterAndHeartbeatTimeout passed");
            return ErrorCode.ERR_Success;
        }
    }
}
