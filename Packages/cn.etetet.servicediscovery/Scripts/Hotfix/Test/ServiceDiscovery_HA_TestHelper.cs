using System;
using System.Collections.Generic;
using System.Linq;
using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// ServiceDiscovery 高可用测试公共工具：
    /// 统一提供测试环境准备、主备等待、状态断言和 Fiber 创建能力。
    /// </summary>
    public static class ServiceDiscovery_HA_TestHelper
    {
        /// <summary>
        /// 快速租约参数：缩短测试时长。
        /// </summary>
        public const int FastLeaseTimeoutMs = 1200;
        public const int FastLeaseRenewIntervalMs = 200;
        private const long WaitPollIntervalMs = 50;

        public static int GetFiberId(int zone, int localSlot)
        {
            return FiberIdHelper.Encode(zone, localSlot);
        }

        public static FiberInstanceId CreateFiberInstanceId(int zone, int localSlot)
        {
            return new FiberInstanceId(GetFiberId(zone, localSlot));
        }

        public static ActorId CreateActorId(int zone, Address address, int localSlot)
        {
            return new ActorId(address, CreateFiberInstanceId(zone, localSlot));
        }

        public static FiberInstanceId CreateFiberInstanceId(Fiber fiber, int localSlot)
        {
            if (fiber == null)
            {
                throw new ArgumentNullException(nameof(fiber));
            }

            return CreateFiberInstanceId(fiber.Zone, localSlot);
        }

        public static ActorId CreateActorId(Fiber fiber, Address address, int localSlot)
        {
            if (fiber == null)
            {
                throw new ArgumentNullException(nameof(fiber));
            }

            return CreateActorId(fiber.Zone, address, localSlot);
        }

        public static int GetServiceDiscoveryAgentFiberId(int zone)
        {
            return ServiceDiscoveryFiberHelper.GetAgentFiberId(zone);
        }

        public static FiberInstanceId CreateServiceDiscoveryAgentFiberInstanceId(int zone)
        {
            return ServiceDiscoveryFiberHelper.CreateAgentFiberInstanceId(zone);
        }

        public static DBComponent GetServiceDiscoveryDB(DBManagerComponent dbManager)
        {
            if (dbManager == null)
            {
                return null;
            }

            return dbManager.GetZoneDB(dbManager.Root().Fiber.Zone);
        }

        public static async ETTask SaveMasterRecordAsync(Scene ownerScene, DBComponent db, string sceneName, ActorId actorId,
            long epoch, long leaseExpireTime, long leaseTimeout, long updateTime)
        {
            if (ownerScene == null)
            {
                throw new ArgumentNullException(nameof(ownerScene));
            }

            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            ServiceDiscovery persistenceOwner = ownerScene.AddComponent<ServiceDiscovery>();
            ServiceDiscoveryMaster masterRecord =
                    persistenceOwner.AddComponentWithId<ServiceDiscoveryMaster>(ServiceDiscoveryPersistenceConst.MasterRecordId);

            try
            {
                masterRecord.SceneName = sceneName ?? string.Empty;
                masterRecord.ActorId = actorId;
                masterRecord.Epoch = epoch;
                masterRecord.LeaseExpireTime = leaseExpireTime;
                masterRecord.LeaseTimeout = leaseTimeout;
                masterRecord.UpdateTime = updateTime;
                await db.Save(masterRecord, ServiceDiscoveryPersistenceConst.MasterCollection);
            }
            finally
            {
                persistenceOwner.RemoveComponent<ServiceDiscoveryMaster>();
                ownerScene.RemoveComponent<ServiceDiscovery>();
            }
        }

        /// <summary>
        /// 获取配置中的 ServiceDiscovery 场景，按 Id 升序返回（通常第一个为主）。
        /// </summary>
        public static List<StartSceneConfig> GetServiceDiscoveryConfigs()
        {
            return World.Instance.GetSingleton<StartSceneConfigCategory>().GetAll().Values
                .Where(c => c.SceneType == nameof(SceneType.ServiceDiscovery))
                .OrderBy(c => c.Id)
                .ToList();
        }

        public static ServiceDiscovery GetActiveMasterServiceDiscovery(Fiber parent)
        {
            if (parent == null)
            {
                return null;
            }

            foreach (StartSceneConfig config in GetServiceDiscoveryConfigs())
            {
                Fiber fiber = parent.GetFiber(GetFiberId(parent.Zone, config.Id));
                ServiceDiscovery serviceDiscovery = fiber?.Root?.GetComponent<ServiceDiscovery>();
                if (serviceDiscovery != null && serviceDiscovery.GetOrAddLease().IsActiveMaster)
                {
                    return serviceDiscovery;
                }
            }

            return null;
        }

        /// <summary>
        /// 确保 AddressSingleton 可用，避免测试场景中 ActorId 构造失败。
        /// </summary>
        public static int EnsureAddressSingletonReady()
        {
            AddressSingleton addressSingleton = AddressSingleton.Instance;
            if (addressSingleton == null)
            {
                addressSingleton = World.Instance.AddSingleton<AddressSingleton>();
            }

            if (!string.IsNullOrEmpty(addressSingleton.InnerIP) &&
                !string.IsNullOrEmpty(addressSingleton.OuterIP) &&
                addressSingleton.InnerPort > 0)
            {
                return 0;
            }

            StartProcessConfig startProcessConfig = World.Instance.GetSingleton<StartProcessConfigCategory>().Get(Options.Instance.Process);
            if (startProcessConfig == null)
            {
                return 2;
            }

            StartMachineConfig startMachineConfig = World.Instance.GetSingleton<StartMachineConfigCategory>()?.Get(startProcessConfig.MachineId);
            addressSingleton.InnerIP ??= startMachineConfig?.InnerIP;
            addressSingleton.OuterIP ??= startMachineConfig?.OuterIP;
            addressSingleton.InnerPort = addressSingleton.InnerPort > 0 ? addressSingleton.InnerPort : startProcessConfig.Port;
            if (string.IsNullOrEmpty(addressSingleton.InnerIP) ||
                string.IsNullOrEmpty(addressSingleton.OuterIP) ||
                addressSingleton.InnerPort <= 0)
            {
                return 3;
            }

            return 0;
        }

        /// <summary>
        /// 将两个节点切换为快速租约配置，并等待主节点可用。
        /// </summary>
        public static async ETTask<int> ConfigureFastLease(ServiceDiscovery sdA, ServiceDiscovery sdB, TimerComponent timer)
        {
            sdA.GetOrAddLease().MasterLeaseTimeout = FastLeaseTimeoutMs;
            sdA.GetOrAddLease().MasterLeaseRenewInterval = FastLeaseRenewIntervalMs;
            sdB.GetOrAddLease().MasterLeaseTimeout = FastLeaseTimeoutMs;
            sdB.GetOrAddLease().MasterLeaseRenewInterval = FastLeaseRenewIntervalMs;

            bool masterReady = await WaitUntilMaster(sdA, timer, 5000);
            if (!masterReady)
            {
                int forceExpireError = await ForceMasterLeaseExpireIfNotSelf(sdA);
                if (forceExpireError == 0)
                {
                    masterReady = await WaitUntilMaster(sdA, timer, 5000);
                }

                if (!masterReady)
                {
                    return 1;
                }
            }

            sdA.GetOrAddLease().CurrentMasterLeaseExpireTime = 0;
            bool renewOk = await sdA.EnsureActiveMasterAsync();
            if (!renewOk)
            {
                return 2;
            }

            return 0;
        }

        public static async ETTask<bool> WaitForAgentBoundToMasterAsync(ServiceDiscoveryAgent agent, ActorId masterActorId,
            TimerComponent timer, long timeoutMs)
        {
            EntityRef<ServiceDiscoveryAgent> agentRef = agent;
            EntityRef<TimerComponent> timerRef = timer;
            long deadline = timer.GetSingleton<TimeInfo>().ServerNow() + timeoutMs;
            while (timer.GetSingleton<TimeInfo>().ServerNow() <= deadline)
            {
                agent = agentRef;
                if (agent == null)
                {
                    return false;
                }

                if (agent.ServiceDiscoveryActorId == masterActorId &&
                    (agent.Status & ServiceDiscoveryAgentStatus.AgentRegistered) != 0)
                {
                    return true;
                }

                timer = timerRef;
                if (timer == null)
                {
                    return false;
                }

                await timer.WaitAsync(WaitPollIntervalMs);
            }

            return false;
        }

        [EnableGetComponent(typeof(TimerComponent))]
        [EnableGetComponent(typeof(CoroutineLockComponent))]
        public static async ETTask<int> ResetServiceDiscoveryStorage(Fiber testFiber)
        {
            if (testFiber == null)
            {
                return 1;
            }

            Scene root = testFiber.Root;
            if (root == null)
            {
                return 2;
            }

            TestFiberDatabaseCleanupComponent cleanupComponent = root.GetComponent<TestFiberDatabaseCleanupComponent>();
            if (cleanupComponent == null)
            {
                return 9;
            }

            cleanupComponent.RegisterLogicalDbName(ServiceDiscoveryPersistenceConst.DBName);

            DBManagerComponent dbManager;
            try
            {
                root.AddComponent<TimerComponent>();
                root.AddComponent<CoroutineLockComponent>();
                dbManager = root.AddComponent<DBManagerComponent>();
            }
            catch (Exception e)
            {
                Log.Console($"reset service discovery storage infrastructure init failed: {e.Message}");
                return 5;
            }

            EntityRef<Scene> rootRef = root;
            EntityRef<DBManagerComponent> dbManagerRef = dbManager;

            try
            {
                await cleanupComponent.CleanupAsync(nameof(ResetServiceDiscoveryStorage));
            }
            catch (Exception e)
            {
                Log.Console($"reset service discovery storage exception: {e.Message}");
                return 5;
            }

            dbManager = dbManagerRef;
            if (dbManager == null)
            {
                return 3;
            }

            DBComponent cleanDb = ServiceDiscovery_HA_TestHelper.GetServiceDiscoveryDB(dbManager);
            if (cleanDb == null)
            {
                return 8;
            }

            root = rootRef;
            if (root == null)
            {
                return 2;
            }

            await ServiceDiscovery_HA_TestHelper.SaveMasterRecordAsync(root, cleanDb, string.Empty, default, 0, 0, 0, 0);
            return 0;
        }

        /// <summary>
        /// 在给定节点集合中等待“唯一主”收敛。
        /// </summary>
        public static async ETTask<(int Error, string MasterSceneName, ActorId MasterActorId, int MasterCount)> WaitSingleMaster(
            List<Fiber> nodes, TimerComponent timer, long timeoutMs)
        {
            if (nodes == null || nodes.Count == 0 || timer == null)
            {
                return (1, string.Empty, default, 0);
            }

            EntityRef<TimerComponent> timerRef = timer;
            long deadline = timer.GetSingleton<TimeInfo>().ServerNow() + timeoutMs;
            while (timer.GetSingleton<TimeInfo>().ServerNow() <= deadline)
            {
                int count = 0;
                string masterSceneName = string.Empty;
                ActorId masterActorId = default;
                for (int i = 0; i < nodes.Count; ++i)
                {
                    Fiber node = nodes[i];
                    if (node == null)
                    {
                        continue;
                    }

                    ServiceDiscovery sd = node.Root.GetComponent<ServiceDiscovery>();
                    if (sd == null)
                    {
                        continue;
                    }

                    EntityRef<ServiceDiscovery> sdRef = sd;
                    try
                    {
                        await sd.RefreshMasterFromDbAsync();
                    }
                    catch (Exception)
                    {
                    }

                    sd = sdRef;
                    if (sd == null || !sd.GetOrAddLease().IsActiveMaster)
                    {
                        continue;
                    }

                    ++count;
                    masterSceneName = sd.Root().Name;
                    masterActorId = sd.Root().GetActorId();
                }

                if (count == 1)
                {
                    return (0, masterSceneName, masterActorId, count);
                }

                timer = timerRef;
                if (timer == null)
                {
                    return (2, string.Empty, default, count);
                }

                await timer.WaitAsync(50);
            }

            return (3, string.Empty, default, 0);
        }

        /// <summary>
        /// 等待某个节点成为可写主节点。
        /// </summary>
        public static async ETTask<bool> WaitUntilMaster(ServiceDiscovery serviceDiscovery, TimerComponent timer, long timeoutMs)
        {
            if (serviceDiscovery == null || timer == null)
            {
                return false;
            }

            EntityRef<ServiceDiscovery> sdRef = serviceDiscovery;
            EntityRef<TimerComponent> timerRef = timer;
            long deadline = timer.GetSingleton<TimeInfo>().ServerNow() + timeoutMs;
            while (timer.GetSingleton<TimeInfo>().ServerNow() <= deadline)
            {
                serviceDiscovery = sdRef;
                if (serviceDiscovery == null)
                {
                    return false;
                }

                try
                {
                    await serviceDiscovery.RefreshMasterFromDbAsync();
                }
                catch (Exception)
                {
                }

                serviceDiscovery = sdRef;
                if (serviceDiscovery == null)
                {
                    return false;
                }

                bool isMaster = false;
                try
                {
                    isMaster = await serviceDiscovery.EnsureActiveMasterAsync();
                }
                catch (Exception)
                {
                    isMaster = false;
                }

                serviceDiscovery = sdRef;
                if (serviceDiscovery == null)
                {
                    return false;
                }

                if (isMaster && serviceDiscovery.GetOrAddLease().IsActiveMaster)
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

        /// <summary>
        /// 直接从 Mongo 查询当前主记录（用于 fencing/epoch 断言）。
        /// </summary>
        public static async ETTask<(int Error, string SceneName, ActorId ActorId, long Epoch, long LeaseExpireTime)> QueryMasterRecord(
            ServiceDiscovery serviceDiscovery)
        {
            if (serviceDiscovery == null)
            {
                return (1, string.Empty, default, 0, 0);
            }

            DBManagerComponent dbManager = serviceDiscovery.Root().GetComponent<DBManagerComponent>();
            DBComponent db = GetServiceDiscoveryDB(dbManager);
            if (db == null)
            {
                return (2, string.Empty, default, 0, 0);
            }

            using ServiceDiscoveryMaster record =
                await db.Query<ServiceDiscoveryMaster>(ServiceDiscoveryPersistenceConst.MasterRecordId,
                    ServiceDiscoveryPersistenceConst.MasterCollection);
            if (record == null)
            {
                return (3, string.Empty, default, 0, 0);
            }

            return (0, record.SceneName, record.ActorId, record.Epoch, record.LeaseExpireTime);
        }

        /// <summary>
        /// 测试兜底：当主节点疑似卡在陈旧租约上时，强制将当前主记录过期以触发重新抢主。
        /// </summary>
        public static async ETTask<int> ForceMasterLeaseExpireIfNotSelf(ServiceDiscovery serviceDiscovery)
        {
            if (serviceDiscovery == null)
            {
                return 1;
            }

            Scene root = serviceDiscovery.Root();
            DBManagerComponent dbManager = root.GetComponent<DBManagerComponent>();
            DBComponent db = GetServiceDiscoveryDB(dbManager);
            if (db == null)
            {
                return 2;
            }

            using ServiceDiscoveryMaster masterRecord = await db.Query<ServiceDiscoveryMaster>(
                ServiceDiscoveryPersistenceConst.MasterRecordId, ServiceDiscoveryPersistenceConst.MasterCollection);
            if (masterRecord == null || masterRecord.ActorId == default)
            {
                return 0;
            }

            ActorId selfActorId = root.GetActorId();
            if (masterRecord.SceneName == root.Name && masterRecord.ActorId == selfActorId)
            {
                return 0;
            }

            long now = serviceDiscovery.GetSingleton<TimeInfo>().ServerNow();
            masterRecord.LeaseExpireTime = now - 1;
            masterRecord.UpdateTime = now;
            await db.Save(masterRecord, ServiceDiscoveryPersistenceConst.MasterCollection);
            return 0;
        }

        /// <summary>
        /// 创建带 ServiceDiscoveryProxy 的普通 Fiber，并完成注册。
        /// </summary>
        [EnableGetComponent(typeof(TimerComponent))]
        [EnableGetComponent(typeof(CoroutineLockComponent))]
        public static async ETTask<Fiber> CreateProxyFiber(Fiber parent, string name, StringKV metadata, bool waitUntilRegistered = true)
        {
            int ensureAgentError = await EnsureServiceDiscoveryAgentFiberAsync(parent);
            if (ensureAgentError != 0)
            {
                Log.Console($"create proxy fiber ensure service discovery agent failed: {ensureAgentError}");
                return null;
            }

            Fiber fiber = await parent.CreateFiber(IdGenerater.Instance.GenerateId(), SceneType.TestEmpty, name);
            Scene root = fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();

            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();
            root.AddComponent<DBManagerComponent>();

            ServiceDiscoveryProxy proxy = root.AddComponent<ServiceDiscoveryProxy>();
            await proxy.RegisterToServiceDiscovery(metadata);

            if (!waitUntilRegistered)
            {
                return fiber;
            }

            ServiceDiscovery master = GetActiveMasterServiceDiscovery(parent);
            TimerComponent timer = master?.Root().TimerComponent;
            bool registered = await WaitForMasterHasService(master, timer, root.Name, 5000);
            if (!registered)
            {
                Log.Console($"create proxy fiber wait register timeout scene: {root.Name}");
                await parent.RemoveFiber(fiber.Id);
                return null;
            }

            return fiber;
        }

        /// <summary>
        /// 测试场景下确保进程级 ServiceDiscoveryAgent Fiber 已创建（与 EntryEvent2_InitServer 一致）。
        /// </summary>
        public static async ETTask<int> EnsureServiceDiscoveryAgentFiberAsync(Fiber parent)
        {
            if (parent == null)
            {
                return 1;
            }

            int agentFiberId = GetServiceDiscoveryAgentFiberId(parent.Zone);
            Fiber agentFiber = parent.GetFiber(agentFiberId);
            if (agentFiber == null)
            {
                int process = Options.Instance.Process;
                string agentName = $"ServiceDiscoveryAgent@{process}@{Options.Instance.ReplicaIndex}";
                try
                {
                    agentFiber = await parent.CreateFiberWithId(agentFiberId, IdGenerater.Instance.GenerateId(), 
                        SceneType.ServiceDiscoveryAgent, agentName);
                }
                catch (Exception e)
                {
                    agentFiber = parent.GetFiber(agentFiberId);
                    if (agentFiber == null)
                    {
                        Log.Console($"ensure service discovery agent fiber failed: {e.Message}");
                        return 2;
                    }
                }
            }

            ServiceDiscoveryAgent agent = agentFiber.Root.GetComponent<ServiceDiscoveryAgent>();
            agent.TriggerBackgroundRegister();
            return 0;
        }

        public static Fiber GetServiceDiscoveryAgentFiber(Fiber parent)
        {
            if (parent == null)
            {
                return null;
            }

            return parent.GetFiber(GetServiceDiscoveryAgentFiberId(parent.Zone));
        }

        public static ServiceDiscoveryAgent GetServiceDiscoveryAgent(Fiber parent)
        {
            return GetServiceDiscoveryAgentFiber(parent)?.Root?.GetComponent<ServiceDiscoveryAgent>();
        }

        /// <summary>
        /// 等待 Proxy 本地缓存出现指定服务。
        /// </summary>
        public static async ETTask<bool> WaitForProxyHasService(ServiceDiscoveryProxy proxy, TimerComponent timer, string sceneName,
            long timeoutMs)
        {
            if (proxy == null || timer == null || string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

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

                if (proxy.SceneNameServices.TryGetValue(sceneName, out EntityRef<ServiceInfo> serviceRef))
                {
                    ServiceInfo serviceInfo = serviceRef;
                    if (serviceInfo != null)
                    {
                        return true;
                    }
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

        /// <summary>
        /// 等待主节点内存状态包含指定服务。
        /// </summary>
        public static async ETTask<bool> WaitForMasterHasService(ServiceDiscovery master, TimerComponent timer, string sceneName,
            long timeoutMs)
        {
            if (master == null || timer == null || string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

            EntityRef<ServiceDiscovery> masterRef = master;
            EntityRef<TimerComponent> timerRef = timer;
            long deadline = timer.GetSingleton<TimeInfo>().ServerNow() + timeoutMs;
            while (timer.GetSingleton<TimeInfo>().ServerNow() <= deadline)
            {
                master = masterRef;
                if (master == null)
                {
                    return false;
                }

                try
                {
                    await master.RefreshMasterFromDbAsync();
                }
                catch (Exception)
                {
                }

                master = masterRef;
                if (master == null)
                {
                    return false;
                }

                if (master.Services.TryGetValue(sceneName, out EntityRef<ServiceInfo> serviceRef))
                {
                    ServiceInfo serviceInfo = serviceRef;
                    if (serviceInfo != null)
                    {
                        return true;
                    }
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

        /// <summary>
        /// 等待主节点持有指定订阅者和过滤器。
        /// </summary>
        public static async ETTask<bool> WaitForMasterHasSubscriber(ServiceDiscovery master, Fiber parent, TimerComponent timer,
            string sceneName, string filterName, long timeoutMs)
        {
            if (master == null || parent == null || timer == null || string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(filterName))
            {
                return false;
            }

            EntityRef<ServiceDiscovery> masterRef = master;
            EntityRef<TimerComponent> timerRef = timer;
            long deadline = timer.GetSingleton<TimeInfo>().ServerNow() + timeoutMs;
            while (timer.GetSingleton<TimeInfo>().ServerNow() <= deadline)
            {
                master = masterRef;
                if (master == null)
                {
                    return false;
                }

                try
                {
                    await master.RefreshMasterFromDbAsync();
                }
                catch (Exception)
                {
                }

                master = masterRef;
                if (master == null)
                {
                    return false;
                }

                ServiceDiscoveryAgent agent = GetServiceDiscoveryAgent(parent);
                if (agent != null &&
                    agent.ProxySubscribers.TryGetValue(sceneName, out ActorId subscriberActorId) &&
                    subscriberActorId != default &&
                    agent.ProxySubscriberFilters.ContainSubKey(sceneName, filterName))
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

        /// <summary>
        /// 按配置创建主/备 ServiceDiscovery 节点；配置不足时使用 fallbackName。
        /// </summary>
        public static async ETTask<Fiber> CreateServiceDiscoveryNodeByConfig(Fiber parent, int configIndex, string fallbackName)
        {
            List<StartSceneConfig> configs = GetServiceDiscoveryConfigs();
            if (configs.Count > configIndex)
            {
                StartSceneConfig config = configs[configIndex];
                return await parent.CreateFiberWithId(config.Id, config.Id, SceneType.ServiceDiscovery, config.Name);
            }

            return await parent.CreateFiber(IdGenerater.Instance.GenerateId(), SceneType.ServiceDiscovery, fallbackName);
        }

        /// <summary>
        /// 等待主节点内存状态不再包含指定服务。
        /// </summary>
        public static async ETTask<bool> WaitForMasterNotHasService(ServiceDiscovery master, TimerComponent timer, string sceneName,
            long timeoutMs)
        {
            if (master == null || timer == null || string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

            EntityRef<ServiceDiscovery> masterRef = master;
            EntityRef<TimerComponent> timerRef = timer;
            long deadline = timer.GetSingleton<TimeInfo>().ServerNow() + timeoutMs;
            while (timer.GetSingleton<TimeInfo>().ServerNow() <= deadline)
            {
                master = masterRef;
                if (master == null)
                {
                    return false;
                }

                try
                {
                    await master.RefreshMasterFromDbAsync();
                }
                catch (Exception)
                {
                }

                master = masterRef;
                if (master == null)
                {
                    return false;
                }

                bool exists = false;
                if (master.Services.TryGetValue(sceneName, out EntityRef<ServiceInfo> serviceRef))
                {
                    ServiceInfo serviceInfo = serviceRef;
                    exists = serviceInfo != null;
                }

                if (!exists)
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

        /// <summary>
        /// 等待 Proxy 本地缓存中移除指定服务。
        /// </summary>
        public static async ETTask<bool> WaitForProxyNotHasService(ServiceDiscoveryProxy proxy, TimerComponent timer, string sceneName,
            long timeoutMs)
        {
            if (proxy == null || timer == null || string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

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

                bool exists = false;
                if (proxy.SceneNameServices.TryGetValue(sceneName, out EntityRef<ServiceInfo> serviceRef))
                {
                    ServiceInfo serviceInfo = serviceRef;
                    exists = serviceInfo != null;
                }

                if (!exists)
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
