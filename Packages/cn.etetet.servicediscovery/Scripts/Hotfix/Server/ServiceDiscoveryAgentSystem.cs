using System;
using System.Collections.Generic;
using System.Linq;

namespace ET.Server
{
    [EntitySystemOf(typeof(ServiceDiscoveryAgent))]
    public static partial class ServiceDiscoveryAgentSystem
    {
        private const int MasterLookupMaxRetryCount = 30;
        private const int MasterResolveRetryBaseInterval = 100;
        private const int MasterResolveRetryMaxInterval = 1000;
        private const int MasterResolveMaxRetryCountWhenEndpointUnavailable = 30;
        private const int RegisterAgentTimeoutMs = 5000;

        [EntitySystem]
        private static void Awake(this ServiceDiscoveryAgent self)
        {
            Scene root = self.Root();
            self.MessageSender = root.GetComponent<MessageSender>();

            self.AddComponent<ServiceDiscoveryAgentProxyHeartbeat>();
            self.EnsureBackgroundRegisterStarted();
        }

        [EntitySystem]
        private static void Destroy(this ServiceDiscoveryAgent self)
        {
        }

        public static bool IsReady(this ServiceDiscoveryAgent self)
        {
            return self != null
                   && self.ServiceDiscoveryActorId != default
                   && self.HasStatus(ServiceDiscoveryAgentStatus.AgentRegistered);
        }

        private static bool HasStatus(this ServiceDiscoveryAgent self, ServiceDiscoveryAgentStatus status)
        {
            return (self.Status & status) != 0;
        }

        private static void SetStatus(this ServiceDiscoveryAgent self, ServiceDiscoveryAgentStatus status)
        {
            self.Status |= status;
        }

        private static void ClearStatus(this ServiceDiscoveryAgent self, ServiceDiscoveryAgentStatus status)
        {
            self.Status &= ~status;
        }

        public static void EnsureMasterHeartbeatComponent(this ServiceDiscoveryAgent self)
        {
            if (self == null)
            {
                return;
            }

            if (self.GetComponent<ServiceDiscoveryAgentMasterHeartbeat>() == null)
            {
                self.AddComponent<ServiceDiscoveryAgentMasterHeartbeat>();
            }
        }

        public static void RemoveMasterHeartbeatComponent(this ServiceDiscoveryAgent self)
        {
            if (self == null)
            {
                return;
            }

            if (self.GetComponent<ServiceDiscoveryAgentMasterHeartbeat>() != null)
            {
                self.RemoveComponent<ServiceDiscoveryAgentMasterHeartbeat>();
            }
        }

        public static void TriggerBackgroundRegister(this ServiceDiscoveryAgent self)
        {
            self.EnsureBackgroundRegisterStarted();
        }

        public static void TriggerBackgroundMutationSync(this ServiceDiscoveryAgent self)
        {
            self.EnsureBackgroundMutationSyncStarted();
        }

        private static void EnsureBackgroundRegisterStarted(this ServiceDiscoveryAgent self)
        {
            if (self == null || self.HasStatus(ServiceDiscoveryAgentStatus.Bootstrapping))
            {
                return;
            }

            self.SetStatus(ServiceDiscoveryAgentStatus.Bootstrapping);
            self.BackgroundRegisterLoopAsync().Coroutine();
        }

        private static void EnsureBackgroundMutationSyncStarted(this ServiceDiscoveryAgent self)
        {
            if (self == null || !self.IsReady() || !self.HasDirtyPublishedScenes() ||
                self.HasStatus(ServiceDiscoveryAgentStatus.MutationSyncing))
            {
                return;
            }

            self.SetStatus(ServiceDiscoveryAgentStatus.MutationSyncing);
            self.BackgroundMutationSyncLoopAsync().Coroutine();
        }

        private static async ETTask BackgroundRegisterLoopAsync(this ServiceDiscoveryAgent self)
        {
            EntityRef<ServiceDiscoveryAgent> selfRef = self;
            int retry = 0;
            try
            {
                while (true)
                {
                    self = selfRef;
                    if (self == null)
                    {
                        return;
                    }

                    int delay;
                    try
                    {
                        bool ready = await self.TryEnsureAgentRegisteredOnceAsync();
                        self = selfRef;
                        if (self == null)
                        {
                            return;
                        }

                        if (ready)
                        {
                            return;
                        }

                        delay = GetMasterResolveRetryDelay(retry);
                        if (retry < MasterLookupMaxRetryCount)
                        {
                            ++retry;
                        }
                    }
                    catch (Exception e)
                    {
                        self = selfRef;
                        if (self == null)
                        {
                            return;
                        }

                        delay = GetMasterResolveRetryDelay(retry);
                        if (retry < MasterLookupMaxRetryCount)
                        {
                            ++retry;
                        }

                        Log.Error($"ServiceDiscoveryAgent background register loop failed scene: {self.Root().Name} error: {e}");
                    }

                    bool waitSucceeded = await WaitBackgroundRegisterRetryDelayAsync(selfRef, delay);
                    if (!waitSucceeded)
                    {
                        return;
                    }
                }
            }
            finally
            {
                self = selfRef;
                if (self != null)
                {
                    self.ClearStatus(ServiceDiscoveryAgentStatus.Bootstrapping);
                }
            }
        }

        private static async ETTask BackgroundMutationSyncLoopAsync(this ServiceDiscoveryAgent self)
        {
            EntityRef<ServiceDiscoveryAgent> selfRef = self;
            int retry = 0;
            try
            {
                while (true)
                {
                    self = selfRef;
                    if (self == null)
                    {
                        return;
                    }

                    if (!self.IsReady() || !self.HasDirtyPublishedScenes())
                    {
                        return;
                    }

                    int delay;
                    try
                    {
                        bool drained = await self.TryFlushDirtyScenesOnceAsync();
                        self = selfRef;
                        if (self == null)
                        {
                            return;
                        }

                        if (drained || !self.IsReady() || !self.HasDirtyPublishedScenes())
                        {
                            return;
                        }

                        delay = 0;
                        retry = 0;
                    }
                    catch (Exception e)
                    {
                        self = selfRef;
                        if (self == null)
                        {
                            return;
                        }

                        delay = GetMasterResolveRetryDelay(retry);
                        if (retry < MasterLookupMaxRetryCount)
                        {
                            ++retry;
                        }

                        Log.Error($"ServiceDiscoveryAgent background mutation sync loop failed scene: {self.Root().Name} error: {e}");
                    }

                    bool waitSucceeded = await WaitBackgroundMutationSyncRetryDelayAsync(selfRef, delay);
                    if (!waitSucceeded)
                    {
                        return;
                    }
                }
            }
            finally
            {
                self = selfRef;
                if (self != null)
                {
                    self.ClearStatus(ServiceDiscoveryAgentStatus.MutationSyncing);
                    if (self.IsReady() && self.HasDirtyPublishedScenes())
                    {
                        self.TriggerBackgroundMutationSync();
                    }
                }
            }
        }

        private static async ETTask<bool> WaitBackgroundRegisterRetryDelayAsync(EntityRef<ServiceDiscoveryAgent> selfRef, int delay)
        {
            ServiceDiscoveryAgent self = selfRef;
            if (self == null)
            {
                return false;
            }

            if (delay <= 0)
            {
                return true;
            }

            try
            {
                await self.Root().TimerComponent.WaitAsync(delay);
                return true;
            }
            catch (Exception e)
            {
                self = selfRef;
                if (self == null)
                {
                    return false;
                }

                Log.Warning(
                    $"ServiceDiscoveryAgent background register retry wait aborted scene: {self.Root().Name} delay: {delay} error: {e.Message}");
                return false;
            }
        }

        private static async ETTask<bool> WaitBackgroundMutationSyncRetryDelayAsync(EntityRef<ServiceDiscoveryAgent> selfRef, int delay)
        {
            ServiceDiscoveryAgent self = selfRef;
            if (self == null)
            {
                return false;
            }

            if (delay <= 0)
            {
                return true;
            }

            try
            {
                await self.Root().TimerComponent.WaitAsync(delay);
                return true;
            }
            catch (Exception e)
            {
                self = selfRef;
                if (self == null)
                {
                    return false;
                }

                Log.Warning(
                    $"ServiceDiscoveryAgent background mutation retry wait aborted scene: {self.Root().Name} delay: {delay} error: {e.Message}");
                return false;
            }
        }

        private static bool NeedRegisterToDiscovery(this ServiceDiscoveryAgent self)
        {
            return self != null
                   && (self.ServiceDiscoveryActorId == default
                       || !self.HasStatus(ServiceDiscoveryAgentStatus.AgentRegistered));
        }

        private static bool HasDirtyPublishedScenes(this ServiceDiscoveryAgent self)
        {
            return self != null && self.DirtyPublishedScenes.Count > 0;
        }

        private static async ETTask<bool> TryEnsureAgentRegisteredOnceAsync(this ServiceDiscoveryAgent self)
        {
            EntityRef<ServiceDiscoveryAgent> selfRef = self;
            using (await self.Root().CoroutineLockComponent.Wait(CoroutineLockType.ServiceDiscoveryAgentInit, self.Id))
            {
                self = selfRef;
                if (self == null)
                {
                    return false;
                }

                if (!self.NeedRegisterToDiscovery())
                {
                    return true;
                }

                if (self.ServiceDiscoveryActorId == default)
                {
                    await self.TryLoadActiveMasterFromDbAsync();
                    self = selfRef;
                    if (self == null)
                    {
                        return false;
                    }

                    if (self.ServiceDiscoveryActorId == default)
                    {
                        return false;
                    }
                }

                Dictionary<string, int> versionSnapshot = self.CapturePublishedSceneVersions();
                bool registered = await self.TryRegisterAgentToDiscoveryAsync(versionSnapshot);
                self = selfRef;
                if (self == null)
                {
                    return false;
                }

                if (!registered)
                {
                    return false;
                }

                if (self.IsReady() && self.HasDirtyPublishedScenes())
                {
                    self.TriggerBackgroundMutationSync();
                }

                return self.IsReady();
            }
        }

        private static async ETTask EnsureReadyForRequestAsync(this ServiceDiscoveryAgent self, string requestName)
        {
            EntityRef<ServiceDiscoveryAgent> selfRef = self;
            self = selfRef;
            if (self == null)
            {
                throw new Exception("service discovery agent disposed");
            }

            if (self.IsReady())
            {
                return;
            }

            int totalRetryCount = MasterResolveMaxRetryCountWhenEndpointUnavailable;
            for (int retry = 0; retry < totalRetryCount; ++retry)
            {
                self = selfRef;
                if (self == null)
                {
                    throw new Exception("service discovery agent disposed");
                }

                if (!self.HasStatus(ServiceDiscoveryAgentStatus.Bootstrapping))
                {
                    self.TriggerBackgroundRegister();
                }

                if (self.IsReady())
                {
                    return;
                }

                if (retry + 1 < totalRetryCount)
                {
                    int delay = GetMasterResolveRetryDelay(retry);
                    await self.Root().TimerComponent.WaitAsync(delay);
                }
            }

            self = selfRef;
            if (self == null)
            {
                throw new Exception("service discovery agent disposed");
            }

            throw new RpcException(ErrorCode.ERR_ServiceDiscoveryMasterUnavailable,
                $"service discovery agent not ready, request: {requestName}, scene: {self.Root().Name}");
        }

        private static async ETTask TryLoadActiveMasterFromDbAsync(this ServiceDiscoveryAgent self)
        {
            if (self == null)
            {
                return;
            }

            EntityRef<ServiceDiscoveryAgent> selfRef = self;
            Scene root = self.Root();
            string rootName = root.Name;
            DBManagerComponent dbManagerComponent = root.GetComponent<DBManagerComponent>();
            if (dbManagerComponent == null)
            {
                return;
            }

            DBComponent dbComponent;
            try
            {
                dbComponent = dbManagerComponent.GetZoneDB(root.Fiber.Zone);
            }
            catch (Exception e)
            {
                Log.Warning(
                    $"ServiceDiscoveryAgent read master from mongo failed scene: {rootName} error: {e.Message}");
                return;
            }
            ServiceDiscoveryMaster masterRecord;
            try
            {
                masterRecord = await dbComponent.Query<ServiceDiscoveryMaster>(
                    ServiceDiscoveryPersistenceConst.MasterRecordId, ServiceDiscoveryPersistenceConst.MasterCollection);
            }
            catch (Exception e)
            {
                Log.Warning(
                    $"ServiceDiscoveryAgent read master from mongo failed scene: {rootName} error: {e.Message}");
                return;
            }

            using (masterRecord)
            {
                self = selfRef;
                if (self == null || masterRecord == null || masterRecord.ActorId == default)
                {
                    return;
                }

                long now = TimeInfo.Instance.ServerNow();
                if (masterRecord.LeaseExpireTime <= now)
                {
                    return;
                }

                if (masterRecord.Epoch > 0 && self.CurrentMasterEpoch > 0 && masterRecord.Epoch < self.CurrentMasterEpoch)
                {
                    return;
                }

                if (masterRecord.Epoch > self.CurrentMasterEpoch)
                {
                    self.CurrentMasterEpoch = masterRecord.Epoch;
                }

                self.SwitchToEndpoint(masterRecord.ActorId, "master-record");
            }
        }

        private static void SwitchToEndpoint(this ServiceDiscoveryAgent self, ActorId targetActorId, string reason)
        {
            if (self.ServiceDiscoveryActorId == targetActorId)
            {
                return;
            }

            ActorId oldActorId = self.ServiceDiscoveryActorId;
            self.ServiceDiscoveryActorId = targetActorId;
            self.ClearStatus(ServiceDiscoveryAgentStatus.AgentRegistered);
            self.RemoveMasterHeartbeatComponent();
            Log.Info(
                $"ServiceDiscoveryAgent switch endpoint scene: {self.Root().Name} reason: {reason} from: {oldActorId} to: {targetActorId}");
        }

        private static void MarkNeedFullRegisterOnCurrentEndpoint(this ServiceDiscoveryAgent self, string reason)
        {
            if (self == null)
            {
                return;
            }

            ActorId currentActorId = self.ServiceDiscoveryActorId;
            self.ClearStatus(ServiceDiscoveryAgentStatus.AgentRegistered);
            self.RemoveMasterHeartbeatComponent();
            self.TriggerBackgroundRegister();
            Log.Info(
                $"ServiceDiscoveryAgent mark full register required scene: {self.Root().Name} reason: {reason} current: {currentActorId}");
        }

        public static void InvalidateEndpointAndTriggerBackgroundRegister(this ServiceDiscoveryAgent self, string reason)
        {
            if (self == null)
            {
                return;
            }

            ActorId oldActorId = self.ServiceDiscoveryActorId;
            self.ServiceDiscoveryActorId = default;
            self.ClearStatus(ServiceDiscoveryAgentStatus.AgentRegistered);
            self.RemoveMasterHeartbeatComponent();
            self.TriggerBackgroundRegister();
            Log.Info(
                $"ServiceDiscoveryAgent invalidate endpoint scene: {self.Root().Name} reason: {reason} old: {oldActorId}");
        }

        public static async ETTask<T> ForwardToDiscoveryByFailover<T>(this ServiceDiscoveryAgent self, IRequest request,
            string requestName) where T : class, IResponse
        {
            EntityRef<ServiceDiscoveryAgent> selfRef = self;
            await self.EnsureReadyForRequestAsync(requestName);
            self = selfRef;
            if (self == null)
            {
                throw new Exception("service discovery agent disposed");
            }

            return await self.ForwardToDiscoveryByFailoverCore<T>(request, requestName);
        }

        private static async ETTask<T> ForwardToDiscoveryByFailoverCore<T>(this ServiceDiscoveryAgent self, IRequest request,
            string requestName) where T : class, IResponse
        {
            EntityRef<ServiceDiscoveryAgent> selfRef = self;
            const int maxAttemptCount = 4;
            for (int attempt = 0; attempt < maxAttemptCount; ++attempt)
            {
                self = selfRef;
                if (self == null)
                {
                    throw new Exception("service discovery agent disposed");
                }

                if (self.ServiceDiscoveryActorId == default)
                {
                    await self.EnsureReadyForRequestAsync(requestName);
                    self = selfRef;
                    if (self == null)
                    {
                        throw new Exception("service discovery agent disposed");
                    }

                    if (!self.IsReady() || self.ServiceDiscoveryActorId == default)
                    {
                        throw new Exception(
                            $"service discovery endpoint unavailable after resolve, request: {requestName}, scene: {self.Root().Name}");
                    }

                    continue;
                }

                ActorId targetActorId = self.ServiceDiscoveryActorId;
                try
                {
                    IResponse rawResponse = await self.MessageSender.Call(targetActorId, request);
                    self = selfRef;
                    if (self == null)
                    {
                        throw new Exception("service discovery agent disposed");
                    }

                    if (rawResponse is not T response)
                    {
                        (rawResponse as MessageObject)?.Dispose();
                        throw new Exception(
                            $"service discovery response type mismatch, request: {requestName}, actual: {rawResponse?.GetType().Name}");
                    }

                    if (response.Error != ErrorCode.ERR_Success)
                    {
                        int error = response.Error;
                        string message = response.Message;
                        if (response is MessageObject messageObject)
                        {
                            messageObject.Dispose();
                        }
                        throw new RpcException(error,
                            $"service discovery response error, request: {requestName}, error: {error}, message: {message}");
                    }

                    return response;
                }
                catch (Exception e)
                {
                    self = selfRef;
                    if (self == null)
                    {
                        throw;
                    }

                    bool shouldResolveMaster = self.ShouldResolveMasterAfterFailure(e);
                    if (shouldResolveMaster)
                    {
                        self.InvalidateEndpointAndTriggerBackgroundRegister("call-failure");
                    }
                    else
                    {
                        Log.Warning(
                            $"ServiceDiscoveryAgent call failed without endpoint fallback scene: {self.Root().Name} request: {requestName} target: {targetActorId} error: {e.Message}");
                        throw;
                    }
                }

                int delay = GetMasterResolveRetryDelay(attempt);
                await self.Root().TimerComponent.WaitAsync(delay);
            }

            throw new Exception($"service discovery request retry exhausted, request: {requestName}");
        }

        private static bool ShouldResolveMasterAfterFailure(this ServiceDiscoveryAgent self, Exception e)
        {

            if (e is RpcException rpcException)
            {
                return ServiceDiscoveryErrorHelper.ShouldTriggerFailover(rpcException.Error);
            }

            return true;
        }

        private static int GetMasterResolveRetryDelay(int retry)
        {
            if (retry <= 0)
            {
                return MasterResolveRetryBaseInterval;
            }

            long delay = (long)MasterResolveRetryBaseInterval << retry;
            if (delay >= MasterResolveRetryMaxInterval)
            {
                return MasterResolveRetryMaxInterval;
            }

            return (int)delay;
        }

        private static async ETTask<bool> TryRegisterAgentToDiscoveryAsync(this ServiceDiscoveryAgent self,
            Dictionary<string, int> versionSnapshot)
        {
            if (self == null)
            {
                return false;
            }

            if (self.ServiceDiscoveryActorId == default)
            {
                return false;
            }

            EntityRef<ServiceDiscoveryAgent> selfRef = self;
            ActorId targetActorId = self.ServiceDiscoveryActorId;
            ServiceAgentRegisterRequest request = ServiceAgentRegisterRequest.Create();
            request.AgentActorId = self.Root().GetActorId();
            if (!ServiceDiscoveryHelper.TryValidateRequiredActorId(request.AgentActorId, nameof(ServiceAgentRegisterRequest),
                    nameof(request.AgentActorId), out string errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            self.AppendOwnedLocalServices(request);

            try
            {
                TimerComponent timer = self.Root().TimerComponent;
                ETTask<IResponse> registerTask = self.MessageSender.Call(targetActorId, request);
                IResponse rawResponse = await registerTask;
                self = selfRef;
                if (self == null)
                {
                    (rawResponse as MessageObject)?.Dispose();
                    return false;
                }

                if (rawResponse is not ServiceAgentRegisterResponse response)
                {
                    (rawResponse as MessageObject)?.Dispose();
                    Log.Warning(
                        $"ServiceDiscoveryAgent register to master response mismatch scene: {self.Root().Name} target: {targetActorId} actual: {rawResponse?.GetType().Name}");
                    self.InvalidateEndpointAndTriggerBackgroundRegister("register-response-mismatch");
                    return false;
                }

                if (response.Error != ErrorCode.ERR_Success)
                {
                    Log.Warning(
                        $"ServiceDiscoveryAgent register to master failed scene: {self.Root().Name} target: {targetActorId} error: {response.Error} message: {response.Message}");
                    bool shouldTriggerFailover = ServiceDiscoveryErrorHelper.ShouldTriggerFailover(response.Error);
                    response.Dispose();
                    if (shouldTriggerFailover)
                    {
                        self.InvalidateEndpointAndTriggerBackgroundRegister("register-failed");
                    }

                    return false;
                }

                Dictionary<string, (ActorId ActorId, StringKV Metadata)> oldServices = self.SnapshotLocalServices();
                Dictionary<string, (ActorId ActorId, StringKV Metadata)> newServices = new();
                foreach (ServiceInfoProto serviceInfoProto in response.Services)
                {
                    if (serviceInfoProto == null || string.IsNullOrEmpty(serviceInfoProto.SceneName) ||
                        serviceInfoProto.ActorId == default)
                    {
                        continue;
                    }

                    newServices[serviceInfoProto.SceneName] =
                        (serviceInfoProto.ActorId, ServiceDiscoveryHelper.CloneMetadata(serviceInfoProto.Metadata));
                }

                self.ClearLocalServices();
                foreach ((string sceneName, (ActorId actorId, StringKV metadata)) in newServices)
                {
                    self.AddOrUpdateLocalService(sceneName, actorId, metadata);
                }

                self.ReplayResyncDiffToProxySubscribers(oldServices, newServices);
                response.Dispose();
                self.SetStatus(ServiceDiscoveryAgentStatus.AgentRegistered);
                self.MarkPublishedScenesSynced(versionSnapshot);
                self.EnsureMasterHeartbeatComponent();
                return true;
            }
            catch (Exception e)
            {
                self = selfRef;
                if (self != null)
                {
                    Log.Warning(
                        $"ServiceDiscoveryAgent register to master deferred scene: {self.Root().Name} target: {targetActorId} error: {e.Message}");
                    self.InvalidateEndpointAndTriggerBackgroundRegister("register-exception");
                }

                return false;
            }
            finally
            {
                request.Dispose();
            }
        }

        private static async ETTask<bool> TryFlushDirtyScenesOnceAsync(this ServiceDiscoveryAgent self)
        {
            if (self == null || !self.IsReady())
            {
                return true;
            }

            EntityRef<ServiceDiscoveryAgent> selfRef = self;
            List<(string SceneName, int Version, bool Exists, ActorId ActorId, StringKV Metadata)> snapshots =
                self.CaptureDirtySceneSnapshots();
            if (snapshots.Count == 0)
            {
                return true;
            }

            foreach ((string sceneName, int version, bool exists, ActorId actorId, StringKV metadata) in snapshots)
            {
                self = selfRef;
                if (self == null || !self.IsReady())
                {
                    return true;
                }

                bool synced = exists
                    ? await self.TrySyncRegisterSceneAsync(sceneName, version, actorId, metadata)
                    : await self.TrySyncUnregisterSceneAsync(sceneName, version);
                self = selfRef;
                if (self == null)
                {
                    return true;
                }

                if (!synced)
                {
                    return true;
                }
            }

            self = selfRef;
            return self == null || !self.HasDirtyPublishedScenes();
        }

        private static async ETTask<bool> TrySyncRegisterSceneAsync(this ServiceDiscoveryAgent self, string sceneName, int version,
            ActorId actorId, StringKV metadata)
        {
            if (self == null || string.IsNullOrEmpty(sceneName) || actorId == default || version <= 0)
            {
                return true;
            }

            if (self.ServiceDiscoveryActorId == default)
            {
                self.MarkNeedFullRegisterOnCurrentEndpoint($"incremental-register-endpoint-missing:{sceneName}");
                return false;
            }

            EntityRef<ServiceDiscoveryAgent> selfRef = self;
            ActorId targetActorId = self.ServiceDiscoveryActorId;
            ServiceRegisterRequest request = ServiceRegisterRequest.Create();
            request.SceneName = sceneName;
            request.ActorId = actorId;
            if (metadata != null)
            {
                foreach ((string key, string value) in metadata)
                {
                    request.Metadata[key] = value;
                }
            }

            try
            {
                TimerComponent timer = self.Root().TimerComponent;
                ETTask<IResponse> registerTask = self.MessageSender.Call(targetActorId, request);
                IResponse rawResponse = await registerTask;
                self = selfRef;
                if (self == null)
                {
                    (rawResponse as MessageObject)?.Dispose();
                    return false;
                }

                if (rawResponse is not ServiceRegisterResponse response)
                {
                    (rawResponse as MessageObject)?.Dispose();
                    throw new Exception(
                        $"service discovery incremental register response mismatch scene: {sceneName}, actual: {rawResponse?.GetType().Name}");
                }

                if (response.Error != ErrorCode.ERR_Success)
                {
                    int error = response.Error;
                    string message = response.Message;
                    response.Dispose();
                    throw new RpcException(error,
                        $"service discovery incremental register failed scene: {sceneName}, error: {error}, message: {message}");
                }

                response.Dispose();
                self.MarkPublishedSceneSynced(sceneName, version);
                return true;
            }
            catch (Exception e)
            {
                self = selfRef;
                if (self != null)
                {
                    Log.Warning(
                        $"ServiceDiscoveryAgent incremental register failed agent: {self.Root().Name} scene: {sceneName} target: {targetActorId} error: {e.Message}");
                    self.HandleIncrementalSyncFailure(e, $"incremental-register:{sceneName}");
                }

                return false;
            }
            finally
            {
                request.Dispose();
            }
        }

        private static async ETTask<bool> TrySyncUnregisterSceneAsync(this ServiceDiscoveryAgent self, string sceneName, int version)
        {
            if (self == null || string.IsNullOrEmpty(sceneName) || version <= 0)
            {
                return true;
            }

            if (self.ServiceDiscoveryActorId == default)
            {
                self.MarkNeedFullRegisterOnCurrentEndpoint($"incremental-unregister-endpoint-missing:{sceneName}");
                return false;
            }

            EntityRef<ServiceDiscoveryAgent> selfRef = self;
            ActorId targetActorId = self.ServiceDiscoveryActorId;
            ServiceUnregisterRequest request = ServiceUnregisterRequest.Create();
            request.SceneName = sceneName;

            try
            {
                TimerComponent timer = self.Root().TimerComponent;
                ETTask<IResponse> unregisterTask = self.MessageSender.Call(targetActorId, request);
                IResponse rawResponse = await unregisterTask;
                self = selfRef;
                if (self == null)
                {
                    (rawResponse as MessageObject)?.Dispose();
                    return false;
                }

                if (rawResponse is not ServiceUnregisterResponse response)
                {
                    (rawResponse as MessageObject)?.Dispose();
                    throw new Exception(
                        $"service discovery incremental unregister response mismatch scene: {sceneName}, actual: {rawResponse?.GetType().Name}");
                }

                if (response.Error != ErrorCode.ERR_Success)
                {
                    int error = response.Error;
                    string message = response.Message;
                    response.Dispose();
                    throw new RpcException(error,
                        $"service discovery incremental unregister failed scene: {sceneName}, error: {error}, message: {message}");
                }

                response.Dispose();
                self.MarkPublishedSceneSynced(sceneName, version);
                return true;
            }
            catch (Exception e)
            {
                self = selfRef;
                if (self != null)
                {
                    Log.Warning(
                        $"ServiceDiscoveryAgent incremental unregister failed agent: {self.Root().Name} scene: {sceneName} target: {targetActorId} error: {e.Message}");
                    self.HandleIncrementalSyncFailure(e, $"incremental-unregister:{sceneName}");
                }

                return false;
            }
            finally
            {
                request.Dispose();
            }
        }

        private static void HandleIncrementalSyncFailure(this ServiceDiscoveryAgent self, Exception e, string reason)
        {
            if (self == null)
            {
                return;
            }

            bool shouldResolveMaster = self.ShouldResolveMasterAfterFailure(e);
            if (shouldResolveMaster)
            {
                self.InvalidateEndpointAndTriggerBackgroundRegister(reason);
                return;
            }

            self.MarkNeedFullRegisterOnCurrentEndpoint(reason);
        }

        private static void AddOrUpdateLocalService(this ServiceDiscoveryAgent self, ServiceInfoProto serviceInfoProto)
        {
            if (serviceInfoProto == null || string.IsNullOrEmpty(serviceInfoProto.SceneName))
            {
                return;
            }

            self.AddOrUpdateLocalService(serviceInfoProto.SceneName, serviceInfoProto.ActorId, serviceInfoProto.Metadata);
        }

        private static void AddOrUpdateLocalService(this ServiceDiscoveryAgent self, string sceneName, ActorId actorId, StringKV metadata)
        {
            if (string.IsNullOrEmpty(sceneName) || actorId == default)
            {
                return;
            }

            if (self.HasSameLocalService(sceneName, actorId, metadata))
            {
                return;
            }

            self.RemoveLocalService(sceneName);

            ServiceInfo serviceInfo = self.AddChild<ServiceInfo, string, ActorId>(sceneName, actorId);
            serviceInfo.Metadata = ServiceDiscoveryHelper.CloneMetadata(metadata);
            self.SceneNameServices[serviceInfo.SceneName] = serviceInfo;
            ServiceDiscoveryHelper.AddToIndexes(self.ServicesIndexs, self.Indexs, serviceInfo.SceneName, serviceInfo.Metadata);
        }

        public static void UpsertLocalServiceAfterRegister(this ServiceDiscoveryAgent self, string sceneName, ActorId actorId,
            StringKV metadata)
        {
            self.UpsertOwnedLocalService(sceneName, actorId, metadata);
            bool changed = !self.HasSameLocalService(sceneName, actorId, metadata);
            if (!changed)
            {
                return;
            }

            self.AddOrUpdateLocalService(sceneName, actorId, metadata);
            using ServiceInfoProto serviceInfoProto = ServiceDiscoveryHelper.CreateServiceInfoProto(sceneName, actorId, metadata);
            self.NotifyProxySubscribers(1, serviceInfoProto);
        }

        public static void RemoveLocalServiceAfterUnregister(this ServiceDiscoveryAgent self, string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            if (self.TryCreateLocalServiceSnapshot(sceneName, out ServiceInfoProto localServiceSnapshot))
            {
                using (localServiceSnapshot)
                {
                    self.NotifyProxySubscribers(2, localServiceSnapshot);
                }
            }

            self.RemoveOwnedLocalService(sceneName);
            self.RemoveLocalService(sceneName);
        }

        private static void RemoveLocalService(this ServiceDiscoveryAgent self, string sceneName)
        {
            ServiceDiscoveryHelper.RemoveLocalServiceCache(self.SceneNameServices, self.ServicesIndexs, self.Indexs, sceneName);
        }

        private static void ClearLocalServices(this ServiceDiscoveryAgent self)
        {
            using ListComponent<string> sceneNames = ListComponent<string>.Create();
            foreach ((string sceneName, EntityRef<ServiceInfo> _) in self.SceneNameServices)
            {
                sceneNames.Add(sceneName);
            }

            foreach (string sceneName in sceneNames)
            {
                self.RemoveLocalService(sceneName);
            }
        }

        private static Dictionary<string, (ActorId ActorId, StringKV Metadata)> SnapshotLocalServices(this ServiceDiscoveryAgent self)
        {
            Dictionary<string, (ActorId ActorId, StringKV Metadata)> snapshot = new();
            foreach ((string sceneName, EntityRef<ServiceInfo> serviceRef) in self.SceneNameServices)
            {
                ServiceInfo serviceInfo = serviceRef;
                if (serviceInfo == null || string.IsNullOrEmpty(sceneName) || serviceInfo.ActorId == default)
                {
                    continue;
                }

                snapshot[sceneName] = (serviceInfo.ActorId, ServiceDiscoveryHelper.CloneMetadata(serviceInfo.Metadata));
            }

            return snapshot;
        }

        private static Dictionary<string, int> CapturePublishedSceneVersions(this ServiceDiscoveryAgent self)
        {
            Dictionary<string, int> snapshot = new();
            if (self == null)
            {
                return snapshot;
            }

            foreach ((string sceneName, int version) in self.PublishedSceneVersions)
            {
                if (string.IsNullOrEmpty(sceneName) || version <= 0)
                {
                    continue;
                }

                snapshot[sceneName] = version;
            }

            return snapshot;
        }

        private static List<(string SceneName, int Version, bool Exists, ActorId ActorId, StringKV Metadata)>
            CaptureDirtySceneSnapshots(this ServiceDiscoveryAgent self)
        {
            List<(string SceneName, int Version, bool Exists, ActorId ActorId, StringKV Metadata)> snapshots = new();
            if (self == null)
            {
                return snapshots;
            }

            foreach (string sceneName in self.DirtyPublishedScenes)
            {
                if (string.IsNullOrEmpty(sceneName))
                {
                    continue;
                }

                if (!self.PublishedSceneVersions.TryGetValue(sceneName, out int version) || version <= 0)
                {
                    continue;
                }

                if (!self.LocalPublishedServices.TryGetValue(sceneName, out (ActorId ActorId, StringKV Metadata) localService)
                    || localService.ActorId == default)
                {
                    snapshots.Add((sceneName, version, false, default, null));
                    continue;
                }

                snapshots.Add((sceneName, version, true, localService.ActorId,
                    ServiceDiscoveryHelper.CloneMetadata(localService.Metadata)));
            }

            return snapshots;
        }

        private static bool HasSameLocalService(this ServiceDiscoveryAgent self, string sceneName, ActorId actorId, StringKV metadata)
        {
            if (self == null || string.IsNullOrEmpty(sceneName) || actorId == default)
            {
                return false;
            }

            if (!self.SceneNameServices.TryGetValue(sceneName, out EntityRef<ServiceInfo> serviceRef))
            {
                return false;
            }

            ServiceInfo serviceInfo = serviceRef;
            if (serviceInfo == null)
            {
                return false;
            }

            return ServiceDiscoveryHelper.IsSameService(serviceInfo.ActorId, serviceInfo.Metadata, actorId, metadata);
        }

        private static bool TryCreateLocalServiceSnapshot(this ServiceDiscoveryAgent self, string sceneName,
            out ServiceInfoProto serviceInfoProto)
        {
            serviceInfoProto = null;
            if (self == null || string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

            if (!self.SceneNameServices.TryGetValue(sceneName, out EntityRef<ServiceInfo> serviceRef))
            {
                return false;
            }

            ServiceInfo serviceInfo = serviceRef;
            if (serviceInfo == null || serviceInfo.ActorId == default)
            {
                return false;
            }

            serviceInfoProto = ServiceDiscoveryHelper.CreateServiceInfoProto(sceneName, serviceInfo.ActorId, serviceInfo.Metadata);
            return true;
        }

        private static void AppendOwnedLocalServices(this ServiceDiscoveryAgent self, ServiceAgentRegisterRequest request)
        {
            if (self == null || request == null)
            {
                return;
            }

            Address ownedAddress = request.AgentActorId.Address;
            foreach ((string sceneName, (ActorId actorId, StringKV metadata)) in self.LocalPublishedServices)
            {
                if (string.IsNullOrEmpty(sceneName) || actorId == default || actorId.Address != ownedAddress)
                {
                    continue;
                }

                request.LocalServices.Add(ServiceDiscoveryHelper.CreateServiceInfoProto(sceneName, actorId, metadata));
            }
        }

        private static void MarkLocalPublishedSceneDirty(this ServiceDiscoveryAgent self, string sceneName)
        {
            if (self == null || string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            if (!self.PublishedSceneVersions.TryGetValue(sceneName, out int version))
            {
                version = 0;
            }

            unchecked
            {
                ++version;
            }

            if (version <= 0)
            {
                version = 1;
            }

            self.PublishedSceneVersions[sceneName] = version;
            self.DirtyPublishedScenes.Add(sceneName);
        }

        private static void MarkPublishedSceneSynced(this ServiceDiscoveryAgent self, string sceneName, int version)
        {
            if (self == null || string.IsNullOrEmpty(sceneName) || version <= 0)
            {
                return;
            }

            if (!self.PublishedSceneVersions.TryGetValue(sceneName, out int currentVersion) || currentVersion != version)
            {
                return;
            }

            self.DirtyPublishedScenes.Remove(sceneName);
            if (!self.LocalPublishedServices.ContainsKey(sceneName))
            {
                self.PublishedSceneVersions.Remove(sceneName);
            }
        }

        private static void MarkPublishedScenesSynced(this ServiceDiscoveryAgent self, Dictionary<string, int> versionSnapshot)
        {
            if (self == null || versionSnapshot == null)
            {
                return;
            }

            foreach ((string sceneName, int version) in versionSnapshot)
            {
                self.MarkPublishedSceneSynced(sceneName, version);
            }
        }

        private static void ScheduleLocalPublishedSceneSync(this ServiceDiscoveryAgent self, string sceneName)
        {
            if (self == null || string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            self.MarkLocalPublishedSceneDirty(sceneName);
            if (self.IsReady())
            {
                self.TriggerBackgroundMutationSync();
                return;
            }

            self.TriggerBackgroundRegister();
        }

        private static void UpsertOwnedLocalService(this ServiceDiscoveryAgent self, string sceneName, ActorId actorId,
            StringKV metadata)
        {
            if (string.IsNullOrEmpty(sceneName) || actorId == default)
            {
                return;
            }

            StringKV copiedMetadata = ServiceDiscoveryHelper.CloneMetadata(metadata);
            if (self.LocalPublishedServices.TryGetValue(sceneName, out (ActorId ActorId, StringKV Metadata) current) &&
                ServiceDiscoveryHelper.IsSameService(current.ActorId, current.Metadata, actorId, copiedMetadata))
            {
                return;
            }

            self.LocalPublishedServices[sceneName] = (actorId, copiedMetadata);
            self.ScheduleLocalPublishedSceneSync(sceneName);
        }

        private static void RemoveOwnedLocalService(this ServiceDiscoveryAgent self, string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            if (!self.LocalPublishedServices.Remove(sceneName))
            {
                return;
            }

            self.ScheduleLocalPublishedSceneSync(sceneName);
        }

        private static void ReplayResyncDiffToProxySubscribers(this ServiceDiscoveryAgent self,
            Dictionary<string, (ActorId ActorId, StringKV Metadata)> oldServices,
            Dictionary<string, (ActorId ActorId, StringKV Metadata)> newServices)
        {
            if (oldServices == null || newServices == null || self.ProxySubscribers.Count == 0)
            {
                return;
            }

            foreach ((string sceneName, (ActorId oldActorId, StringKV oldMetadata)) in oldServices)
            {
                if (string.IsNullOrEmpty(sceneName) || oldActorId == default || newServices.ContainsKey(sceneName))
                {
                    continue;
                }

                using ServiceInfoProto removeInfo = ServiceDiscoveryHelper.CreateServiceInfoProto(sceneName, oldActorId, oldMetadata);
                self.NotifyProxySubscribers(2, removeInfo);
            }

            foreach ((string sceneName, (ActorId newActorId, StringKV newMetadata)) in newServices)
            {
                if (string.IsNullOrEmpty(sceneName) || newActorId == default)
                {
                    continue;
                }

                if (oldServices.TryGetValue(sceneName, out (ActorId ActorId, StringKV Metadata) oldService) &&
                    ServiceDiscoveryHelper.IsSameService(oldService.ActorId, oldService.Metadata, newActorId, newMetadata))
                {
                    continue;
                }

                // 同名服务发生变化时，先用旧元数据回放remove，再用新元数据回放add，
                // 避免订阅过滤条件变化导致proxy残留旧缓存。
                if (oldServices.TryGetValue(sceneName, out oldService))
                {
                    using ServiceInfoProto oldServiceInfo =
                        ServiceDiscoveryHelper.CreateServiceInfoProto(sceneName, oldService.ActorId, oldService.Metadata);
                    self.NotifyProxySubscribers(2, oldServiceInfo);
                }

                using ServiceInfoProto newServiceInfo =
                    ServiceDiscoveryHelper.CreateServiceInfoProto(sceneName, newActorId, newMetadata);
                self.NotifyProxySubscribers(1, newServiceInfo);
            }
        }

        public static int SubscribeProxyServiceChange(this ServiceDiscoveryAgent self, string sceneName, ActorId subscriberActorId,
            string filterName, StringKV filterMetadata, List<ServiceInfoProto> snapshot)
        {
            if (subscriberActorId == default)
            {
                return ErrorCode.ERR_SubscriberActorIdRequired;
            }

            filterMetadata ??= new StringKV();

            self.ProxySubscribers[sceneName] = subscriberActorId;
            if (self.ProxySubscriberFilters.ContainSubKey(sceneName, filterName))
            {
                self.ProxySubscriberFilters.Remove(sceneName, filterName);
            }

            self.ProxySubscriberFilters.Add(sceneName, filterName, ServiceDiscoveryHelper.CloneMetadata(filterMetadata));

            List<ServiceInfo> matchedServices = self.GetServiceInfoByFilter(filterMetadata);
            if (snapshot != null)
            {
                foreach (ServiceInfo serviceInfo in matchedServices)
                {
                    snapshot.Add(serviceInfo.ToProto());
                }
            }

            return ErrorCode.ERR_Success;
        }

        public static void UnsubscribeProxyServiceChange(this ServiceDiscoveryAgent self, string sceneName)
        {
            self.ProxySubscriberFilters.Remove(sceneName);
            self.ProxySubscribers.Remove(sceneName);
        }

        private static void NotifyProxySubscribers(this ServiceDiscoveryAgent self, int changeType, ServiceInfoProto serviceInfoProto)
        {
            if (serviceInfoProto == null || string.IsNullOrEmpty(serviceInfoProto.SceneName) || serviceInfoProto.ActorId == default)
            {
                return;
            }

            MessageSender sender = self.Root().GetComponent<MessageSender>();
            if (sender == null)
            {
                return;
            }

            foreach ((string sceneName, var filters) in self.ProxySubscriberFilters)
            {
                if (!self.ProxySubscribers.TryGetValue(sceneName, out ActorId subscriberActorId) || subscriberActorId == default)
                {
                    continue;
                }

                bool matched = false;
                foreach ((string _, StringKV filter) in filters)
                {
                    if (!ServiceDiscoveryHelper.MatchesMetadataFilter(serviceInfoProto.Metadata, filter))
                    {
                        continue;
                    }

                    matched = true;
                    break;
                }

                if (!matched)
                {
                    continue;
                }

                ServiceChangeNotification notification = ServiceChangeNotification.Create();
                notification.ChangeType = changeType;
                notification.Epoch = self.CurrentMasterEpoch;
                notification.MasterActorId = self.ServiceDiscoveryActorId;

                notification.ServiceInfo.Add(ServiceDiscoveryHelper.CreateServiceInfoProto(serviceInfoProto.SceneName,
                    serviceInfoProto.ActorId, serviceInfoProto.Metadata));
                sender.Send(subscriberActorId, notification);
            }
        }

        public static void OnServiceChangeNotification(this ServiceDiscoveryAgent self, ServiceChangeNotification message)
        {
            if (self == null || message == null || !self.IsReady())
            {
                return;
            }

            if (!self.ShouldAcceptServiceChangeNotification(message))
            {
                return;
            }

            self.OnServiceChangeNotification(message.ChangeType, message.ServiceInfo);
        }

        private static bool ShouldAcceptServiceChangeNotification(this ServiceDiscoveryAgent self, ServiceChangeNotification message)
        {
            if (message == null)
            {
                return false;
            }

            if (message.Epoch <= 0 || message.MasterActorId == default)
            {
                Log.Warning(
                    $"ServiceDiscoveryAgent ignore notification with invalid fencing scene: {self.Root().Name} epoch: {message.Epoch} masterActorId: {message.MasterActorId}");
                return false;
            }

            long currentEpoch = self.CurrentMasterEpoch;
            ActorId currentMasterActorId = self.ServiceDiscoveryActorId;
            if (currentEpoch <= 0 || currentMasterActorId == default)
            {
                Log.Warning(
                    $"ServiceDiscoveryAgent ignore notification because local master view is incomplete scene: {self.Root().Name} currentEpoch: {currentEpoch} currentMaster: {currentMasterActorId}");
                return false;
            }

            if (message.Epoch < currentEpoch)
            {
                Log.Warning(
                    $"ServiceDiscoveryAgent ignore stale notification scene: {self.Root().Name} messageEpoch: {message.Epoch} currentEpoch: {currentEpoch} messageMaster: {message.MasterActorId} currentMaster: {currentMasterActorId}");
                return false;
            }

            if (message.Epoch > currentEpoch)
            {
                Log.Warning(
                    $"ServiceDiscoveryAgent receive newer epoch notification scene: {self.Root().Name} messageEpoch: {message.Epoch} currentEpoch: {currentEpoch} messageMaster: {message.MasterActorId} currentMaster: {currentMasterActorId}");
                self.InvalidateEndpointAndTriggerBackgroundRegister("notification-newer-epoch");
                return false;
            }

            if (message.MasterActorId != currentMasterActorId)
            {
                Log.Warning(
                    $"ServiceDiscoveryAgent ignore notification from unexpected master scene: {self.Root().Name} epoch: {message.Epoch} messageMaster: {message.MasterActorId} currentMaster: {currentMasterActorId}");
                return false;
            }

            return true;
        }

        private static void OnServiceChangeNotification(this ServiceDiscoveryAgent self, int changeType, ServiceInfoProto serviceInfoProto)
        {
            if (!self.IsReady())
            {
                return;
            }

            switch (changeType)
            {
                case 1:
                    if (self.HasSameLocalService(serviceInfoProto.SceneName, serviceInfoProto.ActorId, serviceInfoProto.Metadata))
                    {
                        return;
                    }

                    self.AddOrUpdateLocalService(serviceInfoProto);
                    self.NotifyProxySubscribers(1, serviceInfoProto);
                    break;
                case 2:
                    if (serviceInfoProto != null)
                    {
                        if (!self.SceneNameServices.ContainsKey(serviceInfoProto.SceneName))
                        {
                            return;
                        }

                        self.NotifyProxySubscribers(2, serviceInfoProto);
                        self.RemoveLocalService(serviceInfoProto.SceneName);
                    }

                    break;
            }
        }

        public static void OnServiceChangeNotification(this ServiceDiscoveryAgent self, int changeType,
            List<ServiceInfoProto> serviceInfos)
        {
            if (serviceInfos == null)
            {
                return;
            }

            foreach (ServiceInfoProto serviceInfo in serviceInfos)
            {
                self.OnServiceChangeNotification(changeType, serviceInfo);
            }
        }

        public static List<ServiceInfo> GetServiceInfoByFilter(this ServiceDiscoveryAgent self, StringKV filterMetadata)
        {
            return ServiceDiscoveryHelper.GetServiceInfoByFilter(self.SceneNameServices, self.ServicesIndexs, filterMetadata);
        }

        public static async ETTask EnsureReadyAsync(this ServiceDiscoveryAgent self, string requestName)
        {
            await self.EnsureReadyForRequestAsync(requestName);
        }

        public static async ETTask<List<ServiceInfo>> QueryLocalByFilterAsync(this ServiceDiscoveryAgent self,
            StringKV filterMetadata)
        {
            EntityRef<ServiceDiscoveryAgent> selfRef = self;
            await self.EnsureReadyForRequestAsync(nameof(ServiceQueryRequest));
            self = selfRef;
            if (self == null)
            {
                return new List<ServiceInfo>();
            }

            return self.GetServiceInfoByFilter(filterMetadata);
        }
    }
}
