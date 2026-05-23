using System;
using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(LocationInfo))]
    public static partial class LocationInfoSystem
    {
        [EntitySystem]
        private static void Awake(this LocationInfo self)
        {
            self.TypeStates ??= new Dictionary<int, LocationTypeState>();
        }

        [EntitySystem]
        private static void Destroy(this LocationInfo self)
        {
            self.TypeStates?.Clear();
        }
    }

    [EntitySystemOf(typeof(LocationComponent))]
    public static partial class LocationComponentSystem
    {
        [Event(SceneType.All)]
        public class OnServiceChangeAddService_RefreshLocationPrimary: AEvent<Scene, OnServiceChangeAddService>
        {
            protected override async ETTask Run(Scene scene, OnServiceChangeAddService args)
            {
                LocationComponent locationComponent = scene.GetComponent<LocationComponent>();
                if (locationComponent != null)
                {
                    locationComponent.RefreshPrimaryState();
                }

                await ETTask.CompletedTask;
            }
        }

        [Event(SceneType.All)]
        public class OnServiceChangeRemoveService_RefreshLocationPrimary: AEvent<Scene, OnServiceChangeRemoveService>
        {
            protected override async ETTask Run(Scene scene, OnServiceChangeRemoveService args)
            {
                LocationComponent locationComponent = scene.GetComponent<LocationComponent>();
                if (locationComponent != null)
                {
                    locationComponent.RefreshPrimaryState();
                }

                await ETTask.CompletedTask;
            }
        }

        [EntitySystem]
        private static void Awake(this LocationComponent self)
        {
            self.IsPrimaryLocation = false;
            self.PrimaryLocationSceneName = string.Empty;
        }

        private static string GetCollectionName(this LocationComponent self)
        {
            return LocationPersistenceConst.RouteCollection;
        }

        private static bool IsLocked(LocationTypeState state)
        {
            return state.LockToken != 0;
        }

        private static bool IsExpiredLock(this LocationComponent self, LocationTypeState state)
        {
            return IsLocked(state)
                   && state.LockExpireTime > 0
                   && self.GetSingleton<TimeInfo>().ServerNow() >= state.LockExpireTime;
        }

        private static bool TryGetRouteState(LocationInfo locationInfo, int locationType, out LocationTypeState state)
        {
            state = default;
            return locationInfo != null
                   && locationInfo.TypeStates != null
                   && locationInfo.TypeStates.TryGetValue(locationType, out state);
        }

        private static LocationInfo CreateInfo(this LocationComponent self, long key)
        {
            LocationInfo locationInfo = self.AddChildWithId<LocationInfo>(key);
            locationInfo.TypeStates ??= new Dictionary<int, LocationTypeState>();
            return locationInfo;
        }

        private static void RemoveCachedInfo(this LocationComponent self, long key)
        {
            self.RemoveChild(key);
        }

        private static LocationInfo GetCachedInfo(this LocationComponent self, long key)
        {
            return self.GetChild<LocationInfo>(key);
        }

        private static Dictionary<int, LocationTypeState> CreateStatesSnapshot(LocationInfo locationInfo)
        {
            if (locationInfo == null)
            {
                return null;
            }

            return locationInfo.TypeStates == null
                    ? new Dictionary<int, LocationTypeState>()
                    : new Dictionary<int, LocationTypeState>(locationInfo.TypeStates);
        }

        private static void RestoreStates(this LocationComponent self, long key, Dictionary<int, LocationTypeState> snapshot)
        {
            if (snapshot == null)
            {
                self.RemoveCachedInfo(key);
                return;
            }

            LocationInfo locationInfo = self.GetCachedInfo(key) ?? self.CreateInfo(key);
            locationInfo.TypeStates.Clear();
            foreach ((int locationType, LocationTypeState state) in snapshot)
            {
                locationInfo.TypeStates[locationType] = state;
            }
        }

        private static DBComponent GetDBComponent(this LocationComponent self)
        {
            Scene root = self.Root();
            DBManagerComponent dbManagerComponent = root.GetComponent<DBManagerComponent>();
            if (dbManagerComponent == null)
            {
                throw new Exception($"location db manager not found scene: {root.Name}");
            }

            return dbManagerComponent.GetZoneDB(root.Fiber.Zone);
        }

        private static async ETTask SaveInfoToDB(this LocationComponent self, LocationInfo locationInfo)
        {
            if (locationInfo == null)
            {
                return;
            }

            long key = locationInfo.Id;
            EntityRef<LocationComponent> selfRef = self;
            EntityRef<LocationInfo> locationInfoRef = locationInfo;
            DBComponent dbComponent = self.GetDBComponent();
            EntityRef<DBComponent> dbComponentRef = dbComponent;
            Scene root = self.Root();
            EntityRef<Scene> rootRef = root;

            using (await root.CoroutineLockComponent.Wait(CoroutineLockType.LocationPersistence, key))
            {
                root = rootRef;
                self = selfRef;
                locationInfo = locationInfoRef;
                dbComponent = dbComponentRef;
                if (root == null || self == null || locationInfo == null || dbComponent == null)
                {
                    return;
                }

                await dbComponent.Save(locationInfo, self.GetCollectionName());
            }
        }

        private static async ETTask RemoveInfoFromDB(this LocationComponent self, long key)
        {
            EntityRef<LocationComponent> selfRef = self;
            DBComponent dbComponent = self.GetDBComponent();
            EntityRef<DBComponent> dbComponentRef = dbComponent;
            Scene root = self.Root();
            EntityRef<Scene> rootRef = root;

            using (await root.CoroutineLockComponent.Wait(CoroutineLockType.LocationPersistence, key))
            {
                root = rootRef;
                self = selfRef;
                dbComponent = dbComponentRef;
                if (root == null || self == null || dbComponent == null)
                {
                    return;
                }

                await dbComponent.Remove<LocationInfo>(key, self.GetCollectionName());
            }
        }

        private static async ETTask<LocationInfo> LoadInfoFromDB(this LocationComponent self, long key)
        {
            EntityRef<LocationComponent> selfRef = self;
            DBComponent dbComponent = self.GetDBComponent();
            string collectionName = self.GetCollectionName();

            using LocationInfo loadedInfo = await dbComponent.Query<LocationInfo>(key, collectionName);
            self = selfRef;
            if (self == null || loadedInfo == null || loadedInfo.TypeStates == null || loadedInfo.TypeStates.Count == 0)
            {
                return null;
            }

            LocationInfo locationInfo = self.CreateInfo(key);
            foreach ((int locationType, LocationTypeState state) in loadedInfo.TypeStates)
            {
                locationInfo.TypeStates[locationType] = state;
            }

            return locationInfo;
        }

        private static async ETTask<LocationInfo> GetOrLoadInfo(this LocationComponent self, long key)
        {
            LocationInfo cached = self.GetCachedInfo(key);
            if (cached != null)
            {
                cached.TypeStates ??= new Dictionary<int, LocationTypeState>();
                return cached;
            }

            return await self.LoadInfoFromDB(key);
        }

        public static void ClearAllCache(this LocationComponent self)
        {
            using ListComponent<long> ids = ListComponent<long>.Create();
            foreach ((long key, Entity _) in self.Children)
            {
                ids.Add(key);
            }

            foreach (long key in ids)
            {
                self.RemoveCachedInfo(key);
            }
        }

        public static async ETTask Add(this LocationComponent self, int locationType, long key, ActorId actorId)
        {
            EntityRef<LocationComponent> selfRef = self;
            using (await self.Root().CoroutineLockComponent.Wait(CoroutineLockType.Location, key))
            {
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                LocationInfo locationInfo = await self.GetOrLoadInfo(key);
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                locationInfo ??= self.CreateInfo(key);
                Dictionary<int, LocationTypeState> snapshot = CreateStatesSnapshot(locationInfo);

                if (TryGetRouteState(locationInfo, locationType, out LocationTypeState currentState)
                    && self.IsExpiredLock(currentState))
                {
                    currentState.LockToken = default;
                    currentState.LockExpireTime = default;
                    locationInfo.TypeStates[locationType] = currentState;
                }

                if (TryGetRouteState(locationInfo, locationType, out currentState) && IsLocked(currentState))
                {
                    throw new RpcException(ErrorCode.ERR_LocationAlreadyLocked,
                        $"location add rejected by lock type: {locationType} key: {key} actorId: {currentState.ActorId} lockToken: {currentState.LockToken}");
                }

                bool oldExists = TryGetRouteState(locationInfo, locationType, out LocationTypeState oldState);
                locationInfo.TypeStates[locationType] = new LocationTypeState { ActorId = actorId };

                try
                {
                    await self.SaveInfoToDB(locationInfo);
                }
                catch
                {
                    self = selfRef;
                    if (self != null)
                    {
                        self.RestoreStates(key, snapshot);
                    }

                    throw;
                }

                if (oldExists)
                {
                    Log.Info($"location replace type: {locationType} key: {key} oldActorId: {oldState.ActorId} newActorId: {actorId}");
                    return;
                }

                Log.Info($"location add type: {locationType} key: {key} actorId: {actorId}");
            }
        }

        public static async ETTask Remove(this LocationComponent self, int locationType, long key, ActorId expectedActorId = default)
        {
            EntityRef<LocationComponent> selfRef = self;
            using (await self.Root().CoroutineLockComponent.Wait(CoroutineLockType.Location, key))
            {
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                LocationInfo locationInfo = await self.GetOrLoadInfo(key);
                self = selfRef;
                if (self == null || locationInfo == null)
                {
                    return;
                }

                if (!TryGetRouteState(locationInfo, locationType, out LocationTypeState state))
                {
                    return;
                }

                if (expectedActorId != default && state.ActorId != expectedActorId)
                {
                    Log.Warning(
                        $"location remove skip by actor mismatch type: {locationType} key: {key} expectedActorId: {expectedActorId} currentActorId: {state.ActorId}");
                    return;
                }

                Dictionary<int, LocationTypeState> snapshot = CreateStatesSnapshot(locationInfo);
                if (self.IsExpiredLock(state))
                {
                    state.LockToken = default;
                    state.LockExpireTime = default;
                    locationInfo.TypeStates[locationType] = state;
                }

                if (IsLocked(state))
                {
                    if (expectedActorId != default)
                    {
                        Log.Warning(
                            $"location remove skip by active lock type: {locationType} key: {key} expectedActorId: {expectedActorId} actorId: {state.ActorId} lockToken: {state.LockToken}");
                        return;
                    }

                    throw new RpcException(ErrorCode.ERR_LocationAlreadyLocked,
                        $"location remove rejected by lock type: {locationType} key: {key} actorId: {state.ActorId} lockToken: {state.LockToken}");
                }

                locationInfo.TypeStates.Remove(locationType);

                try
                {
                    if (locationInfo.TypeStates.Count == 0)
                    {
                        self.RemoveCachedInfo(key);
                        await self.RemoveInfoFromDB(key);
                    }
                    else
                    {
                        await self.SaveInfoToDB(locationInfo);
                    }
                }
                catch
                {
                    self = selfRef;
                    if (self != null)
                    {
                        self.RestoreStates(key, snapshot);
                    }

                    throw;
                }

                Log.Info($"location remove type: {locationType} key: {key}");
            }
        }

        public static async ETTask<long> Lock(this LocationComponent self, int locationType, long key, ActorId actorId, int time = 0)
        {
            EntityRef<LocationComponent> selfRef = self;
            using (await self.Root().CoroutineLockComponent.Wait(CoroutineLockType.Location, key))
            {
                self = selfRef;
                if (self == null)
                {
                    return default;
                }

                LocationInfo locationInfo = await self.GetOrLoadInfo(key);
                self = selfRef;
                if (self == null)
                {
                    return default;
                }

                locationInfo ??= self.CreateInfo(key);
                Dictionary<int, LocationTypeState> snapshot = CreateStatesSnapshot(locationInfo);
                bool exists = TryGetRouteState(locationInfo, locationType, out LocationTypeState state);

                if (exists && self.IsExpiredLock(state))
                {
                    state.LockToken = default;
                    state.LockExpireTime = default;
                    locationInfo.TypeStates[locationType] = state;
                }

                if (IsLocked(state))
                {
                    if (state.ActorId == actorId)
                    {
                        Log.Warning(
                            $"location lock idempotent type: {locationType} key: {key} actorId: {actorId} lockToken: {state.LockToken}");
                        return state.LockToken;
                    }

                    throw new RpcException(ErrorCode.ERR_LocationAlreadyLocked,
                        $"location lock already exists type: {locationType} key: {key} actorId: {state.ActorId} requestActorId: {actorId}");
                }

                if (exists && state.ActorId != default && state.ActorId != actorId)
                {
                    throw new RpcException(ErrorCode.ERR_LocationLockOwnerMismatch,
                        $"location lock owner mismatch type: {locationType} key: {key} requestActorId: {actorId} holdActorId: {state.ActorId}");
                }

                long lockToken = IdGenerater.Instance.GenerateId();
                long lockExpireTime = time > 0 ? self.GetSingleton<TimeInfo>().ServerNow() + time : 0;
                state.ActorId = actorId;
                state.LockToken = lockToken;
                state.LockExpireTime = lockExpireTime;
                locationInfo.TypeStates[locationType] = state;

                try
                {
                    await self.SaveInfoToDB(locationInfo);
                }
                catch
                {
                    self = selfRef;
                    if (self != null)
                    {
                        self.RestoreStates(key, snapshot);
                    }

                    throw;
                }

                Log.Info(
                    $"location lock type: {locationType} key: {key} actorId: {actorId} lockToken: {lockToken} lockExpireTime: {lockExpireTime}");

                return lockToken;
            }
        }

        public static async ETTask UnLock(this LocationComponent self, int locationType, long key, ActorId oldActorId, ActorId newActorId,
        long lockToken)
        {
            EntityRef<LocationComponent> selfRef = self;
            using (await self.Root().CoroutineLockComponent.Wait(CoroutineLockType.Location, key))
            {
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                LocationInfo locationInfo = await self.GetOrLoadInfo(key);
                self = selfRef;
                if (self == null || locationInfo == null || !TryGetRouteState(locationInfo, locationType, out LocationTypeState state))
                {
                    throw new RpcException(ErrorCode.ERR_LocationLockNotFound,
                        $"location unlock not found type: {locationType} key: {key} oldActorId: {oldActorId}");
                }

                Dictionary<int, LocationTypeState> snapshot = CreateStatesSnapshot(locationInfo);
                if (self.IsExpiredLock(state))
                {
                    state.LockToken = default;
                    state.LockExpireTime = default;
                    locationInfo.TypeStates[locationType] = state;

                    try
                    {
                        await self.SaveInfoToDB(locationInfo);
                    }
                    catch
                    {
                        self = selfRef;
                        if (self != null)
                        {
                            self.RestoreStates(key, snapshot);
                        }

                        throw;
                    }

                    throw new RpcException(ErrorCode.ERR_LocationLockNotFound,
                        $"location unlock expired type: {locationType} key: {key} oldActorId: {oldActorId}");
                }

                if (!IsLocked(state))
                {
                    throw new RpcException(ErrorCode.ERR_LocationLockNotFound,
                        $"location unlock not found type: {locationType} key: {key} oldActorId: {oldActorId}");
                }

                if (lockToken == 0 || state.LockToken != lockToken)
                {
                    throw new RpcException(ErrorCode.ERR_LocationLockTokenMismatch,
                        $"location unlock token mismatch type: {locationType} key: {key} oldActorId: {oldActorId} requestToken: {lockToken} holdToken: {state.LockToken}");
                }

                if (oldActorId != state.ActorId)
                {
                    throw new RpcException(ErrorCode.ERR_LocationLockOwnerMismatch,
                        $"location unlock old actor mismatch type: {locationType} key: {key} requestOldActorId: {oldActorId} holdActorId: {state.ActorId}");
                }

                Log.Info(
                    $"location unlock type: {locationType} key: {key} actorId: {oldActorId} newActorId: {newActorId} lockToken: {lockToken}");

                state.ActorId = newActorId;
                state.LockToken = default;
                state.LockExpireTime = default;
                locationInfo.TypeStates[locationType] = state;

                try
                {
                    await self.SaveInfoToDB(locationInfo);
                }
                catch
                {
                    self = selfRef;
                    if (self != null)
                    {
                        self.RestoreStates(key, snapshot);
                    }

                    throw;
                }
            }
        }

        public static async ETTask<ActorId> Get(this LocationComponent self, int locationType, long key)
        {
            EntityRef<LocationComponent> selfRef = self;
            using (await self.Root().CoroutineLockComponent.Wait(CoroutineLockType.Location, key))
            {
                self = selfRef;
                if (self == null)
                {
                    return default;
                }

                LocationInfo locationInfo = await self.GetOrLoadInfo(key);
                self = selfRef;
                if (self == null || locationInfo == null)
                {
                    return default;
                }

                if (!TryGetRouteState(locationInfo, locationType, out LocationTypeState state))
                {
                    return default;
                }

                if (self.IsExpiredLock(state))
                {
                    Dictionary<int, LocationTypeState> snapshot = CreateStatesSnapshot(locationInfo);
                    state.LockToken = default;
                    state.LockExpireTime = default;
                    locationInfo.TypeStates[locationType] = state;

                    try
                    {
                        await self.SaveInfoToDB(locationInfo);
                    }
                    catch
                    {
                        self = selfRef;
                        if (self != null)
                        {
                            self.RestoreStates(key, snapshot);
                        }

                        throw;
                    }
                }
                else if (IsLocked(state))
                {
                    throw new RpcException(ErrorCode.ERR_LocationGetRetry,
                        $"location get retry type: {locationType} key: {key} actorId: {state.ActorId} lockToken: {state.LockToken}");
                }

                Log.Info($"location get type: {locationType} key: {key} actorId: {state.ActorId}");
                return state.ActorId;
            }
        }

        public static void RefreshPrimaryState(this LocationComponent self)
        {
            Scene root = self.Root();
            if (root == null)
            {
                return;
            }

            ServiceDiscoveryProxy serviceDiscoveryProxy = root.GetComponent<ServiceDiscoveryProxy>();
            if (serviceDiscoveryProxy == null)
            {
                return;
            }

            List<ServiceInfo> serviceInfos = serviceDiscoveryProxy.GetBySceneType(SceneType.Location);
            string primarySceneName = string.Empty;
            bool isPrimaryLocation = false;
            if (serviceInfos.Count > 0)
            {
                primarySceneName = LocationPrimaryHelper.SelectStablePrimarySceneName(serviceInfos);
                isPrimaryLocation = string.Equals(root.Name, primarySceneName, StringComparison.Ordinal);
            }

            bool oldIsPrimaryLocation = self.IsPrimaryLocation;
            string oldPrimarySceneName = self.PrimaryLocationSceneName;
            self.IsPrimaryLocation = isPrimaryLocation;
            self.PrimaryLocationSceneName = primarySceneName;

            if (oldIsPrimaryLocation && !isPrimaryLocation)
            {
                self.ClearAllCache();
                Log.Warning($"location component demoted to backup scene: {root.Name} primary: {primarySceneName}");
                return;
            }

            if (oldIsPrimaryLocation != isPrimaryLocation || !string.Equals(oldPrimarySceneName, primarySceneName, StringComparison.Ordinal))
            {
                Log.Info($"location component primary state changed scene: {root.Name} isPrimary: {isPrimaryLocation} primary: {primarySceneName}");
            }
        }

        public static bool EnsurePrimary(this LocationComponent self, IResponse response)
        {
            if (response == null)
            {
                return false;
            }

            if (self != null && self.IsPrimaryLocation)
            {
                return true;
            }

            if (self != null && !string.IsNullOrEmpty(self.PrimaryLocationSceneName))
            {
                Scene root = self.Root();
                if (root != null)
                {
                    ServiceDiscoveryProxy serviceDiscoveryProxy = root.GetComponent<ServiceDiscoveryProxy>();
                    if (serviceDiscoveryProxy != null)
                    {
                        try
                        {
                            ServiceInfo primaryServiceInfo = serviceDiscoveryProxy.GetServiceInfo(self.PrimaryLocationSceneName);
                            if (primaryServiceInfo != null && primaryServiceInfo.ActorId == root.GetActorId())
                            {
                                return true;
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                response.Error = ErrorCode.ERR_LocationFollowerRejected;
                response.Message = $"location is follower currentPrimary: {self.PrimaryLocationSceneName}";
                return false;
            }

            response.Error = ErrorCode.ERR_LocationPrimaryUnavailable;
            response.Message = "location primary unavailable";
            return false;
        }
    }
}
