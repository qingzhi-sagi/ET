using System;
using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(LocationInfo))]
    public static partial class LocationInfoSystem
    {
        [EntitySystem]
        private static void Awake(this LocationInfo self, ActorId actorId)
        {
            self.ActorId = actorId;
            self.LockToken = default;
            self.LockExpireTime = default;
        }

        [EntitySystem]
        private static void Destroy(this LocationInfo self)
        {
            self.ActorId = default;
            self.LockToken = default;
            self.LockExpireTime = default;
        }
    }

    [EntitySystemOf(typeof(LocationOneType))]
    public static partial class LocationOneTypeSystem
    {
        [EntitySystem]
        private static void Awake(this LocationOneType self)
        {
        }

        private static string GetCollectionName(this LocationOneType self)
        {
            return LocationPersistenceConst.RouteCollection;
        }

        private static bool IsLocked(LocationInfo locationInfo)
        {
            return locationInfo != null && locationInfo.LockToken != 0;
        }

        private static bool IsExpiredLock(LocationInfo locationInfo)
        {
            return IsLocked(locationInfo)
                   && locationInfo.LockExpireTime > 0
                   && locationInfo.GetSingleton<TimeInfo>().ServerNow() >= locationInfo.LockExpireTime;
        }

        private static bool TryGetCachedInfo(this LocationOneType self, long key, out LocationInfo locationInfo)
        {
            locationInfo = self.GetChild<LocationInfo>(key);
            return locationInfo != null;
        }

        private static LocationInfo CreateInfo(this LocationOneType self, long key, ActorId actorId)
        {
            return self.AddChildWithId<LocationInfo, ActorId>(key, actorId);
        }

        public static void RemoveCachedInfo(this LocationOneType self, long key)
        {
            self.RemoveChild(key);
        }

        private static async ETTask<LocationInfo> ClearExpiredLockIfNeeded(
            this LocationOneType self,
            EntityRef<LocationOneType> selfRef,
            long key,
            LocationInfo locationInfo,
            string reason)
        {
            if (!IsExpiredLock(locationInfo))
            {
                return locationInfo;
            }

            long oldLockToken = locationInfo.LockToken;
            long oldLockExpireTime = locationInfo.LockExpireTime;
            locationInfo.LockToken = default;
            locationInfo.LockExpireTime = default;
            EntityRef<LocationInfo> locationInfoRef = locationInfo;

            try
            {
                await self.SaveStateToDB(locationInfo);
            }
            catch
            {
                locationInfo = locationInfoRef;
                if (locationInfo == null)
                {
                    throw;
                }
                locationInfo.LockToken = oldLockToken;
                locationInfo.LockExpireTime = oldLockExpireTime;
                throw;
            }

            self = selfRef;
            if (self == null)
            {
                return null;
            }

            locationInfo = locationInfoRef;
            if (locationInfo == null)
            {
                return null;
            }

            Log.Warning(
                $"location clear expired lock key: {key} actorId: {locationInfo.ActorId} lockToken: {oldLockToken} lockExpireTime: {oldLockExpireTime} reason: {reason}");
            return locationInfo;
        }

        private static LocationInfo ApplyLoadedStateIfNeeded(this LocationOneType self, long key, LocationState state)
        {
            if (self.TryGetCachedInfo(key, out LocationInfo current))
            {
                return current;
            }

            if (!state.Exists)
            {
                return null;
            }

            LocationInfo locationInfo = self.CreateInfo(key, state.ActorId);
            locationInfo.ActorId = state.ActorId;
            locationInfo.LockToken = state.LockToken;
            locationInfo.LockExpireTime = state.LockExpireTime;
            return locationInfo;
        }

        private static LocationState CreateStateSnapshot(this LocationOneType self, LocationInfo locationInfo)
        {
            if (locationInfo == null)
            {
                return default;
            }

            return new LocationState
            {
                Exists = true,
                ActorId = locationInfo.ActorId,
                LockToken = locationInfo.LockToken,
                LockExpireTime = locationInfo.LockExpireTime,
            };
        }

        private static void RestoreCachedState(this LocationOneType self, long key, LocationState state)
        {
            if (!state.Exists)
            {
                self.RemoveCachedInfo(key);
                return;
            }

            LocationInfo locationInfo = self.TryGetCachedInfo(key, out LocationInfo current)
                    ? current
                    : self.CreateInfo(key, state.ActorId);
            locationInfo.ActorId = state.ActorId;
            locationInfo.LockToken = state.LockToken;
            locationInfo.LockExpireTime = state.LockExpireTime;
        }

        private static DBComponent GetDBComponent(this LocationOneType self)
        {
            Scene root = self.Root();
            DBManagerComponent dbManagerComponent = root.GetComponent<DBManagerComponent>();
            if (dbManagerComponent == null)
            {
                throw new Exception($"location db manager not found scene: {root.Name}");
            }

            return dbManagerComponent.GetZoneDB(root.Fiber.Zone);
        }

        private static async ETTask SaveStateToDB(this LocationOneType self, LocationInfo locationInfo)
        {
            if (locationInfo == null)
            {
                return;
            }

            long key = locationInfo.Id;
            EntityRef<LocationOneType> selfRef = self;
            EntityRef<LocationInfo> locationInfoRef = locationInfo;
            DBComponent dbComponent = self.GetDBComponent();
            if (dbComponent == null)
            {
                return;
            }
            EntityRef<DBComponent> dbComponentRef = dbComponent;
            self = selfRef;
            if (self == null)
            {
                return;
            }

            Scene root = self.Root();
            EntityRef<Scene> rootRef = root;
            long persistenceLockType = (self.Id << 32) | CoroutineLockType.LocationPersistence;
            using (await root.CoroutineLockComponent.Wait(persistenceLockType, key))
            {
                root = rootRef;
                if (root == null)
                {
                    return;
                }

                self = selfRef;
                dbComponent = dbComponentRef;
                if (self == null || dbComponent == null)
                {
                    return;
                }

                locationInfo = locationInfoRef;
                if (locationInfo == null)
                {
                    return;
                }

                string collectionName = self.GetCollectionName();
                await dbComponent.Save(locationInfo, collectionName);
            }
        }

        private static async ETTask RemoveStateFromDB(this LocationOneType self, long key)
        {
            EntityRef<LocationOneType> selfRef = self;
            DBComponent dbComponent = self.GetDBComponent();
            if (dbComponent == null)
            {
                return;
            }
            EntityRef<DBComponent> dbComponentRef = dbComponent;
            self = selfRef;
            if (self == null)
            {
                return;
            }

            Scene root = self.Root();
            EntityRef<Scene> rootRef = root;
            long persistenceLockType = (self.Id << 32) | CoroutineLockType.LocationPersistence;
            using (await root.CoroutineLockComponent.Wait(persistenceLockType, key))
            {
                root = rootRef;
                if (root == null)
                {
                    return;
                }

                self = selfRef;
                dbComponent = dbComponentRef;
                if (self == null || dbComponent == null)
                {
                    return;
                }

                string collectionName = self.GetCollectionName();
                await dbComponent.Remove<LocationInfo>(key, collectionName);
            }
        }

        private static async ETTask<LocationState> LoadStateFromDB(this LocationOneType self, long key)
        {
            EntityRef<LocationOneType> selfRef = self;
            DBComponent dbComponent = self.GetDBComponent();
            self = selfRef;
            if (self == null || dbComponent == null)
            {
                return default;
            }

            string collectionName = self.GetCollectionName();
            using LocationInfo locationInfo = await dbComponent.Query<LocationInfo>(key, collectionName);
            if (locationInfo == null)
            {
                return default;
            }

            return new LocationState()
            {
                Exists = true,
                ActorId = locationInfo.ActorId,
                LockToken = locationInfo.LockToken,
                LockExpireTime = locationInfo.LockExpireTime,
            };
        }

        private static async ETTask<LocationInfo> GetOrLoadInfo(this LocationOneType self, EntityRef<LocationOneType> selfRef, long key)
        {
            if (self.TryGetCachedInfo(key, out LocationInfo cached))
            {
                return cached;
            }

            LocationState state = await self.LoadStateFromDB(key);
            self = selfRef;
            if (self == null)
            {
                return null;
            }

            return self.ApplyLoadedStateIfNeeded(key, state);
        }

        public static async ETTask Add(this LocationOneType self, long key, ActorId instanceId)
        {
            EntityRef<LocationOneType> selfRef = self;
            long coroutineLockType = (self.Id << 32) | CoroutineLockType.Location;
            using (await self.Root().CoroutineLockComponent.Wait(coroutineLockType, key))
            {
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                LocationInfo locationInfo = await self.GetOrLoadInfo(selfRef, key);
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                locationInfo = await self.ClearExpiredLockIfNeeded(selfRef, key, locationInfo, "add-or-replace");
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                if (IsLocked(locationInfo))
                {
                    throw new RpcException(ErrorCode.ERR_LocationAlreadyLocked,
                        $"location add replace rejected by lock key: {key} requestActorId: {instanceId} holdActorId: {locationInfo.ActorId} lockToken: {locationInfo.LockToken}");
                }

                LocationState oldState = self.CreateStateSnapshot(locationInfo);
                if (locationInfo != null)
                {
                    self.RemoveCachedInfo(key);
                }

                locationInfo = self.CreateInfo(key, instanceId);
                EntityRef<LocationInfo> locationInfoRef = locationInfo;

                try
                {
                    await self.SaveStateToDB(locationInfo);
                }
                catch
                {
                    self = selfRef;
                    if (self != null)
                    {
                        locationInfo = locationInfoRef;
                        if (locationInfo != null)
                        {
                            self.RemoveCachedInfo(key);
                        }
                        self.RestoreCachedState(key, oldState);
                    }

                    throw;
                }

                self = selfRef;
                if (self == null)
                {
                    return;
                }

                if (oldState.Exists)
                {
                    Log.Info($"location replace key: {key} oldActorId: {oldState.ActorId} newActorId: {instanceId}");
                    return;
                }

                Log.Info($"location add key: {key} instanceId: {instanceId}");
            }
        }

        public static async ETTask Remove(this LocationOneType self, long key, ActorId expectedActorId = default)
        {
            EntityRef<LocationOneType> selfRef = self;
            long coroutineLockType = (self.Id << 32) | CoroutineLockType.Location;
            using (await self.Root().CoroutineLockComponent.Wait(coroutineLockType, key))
            {
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                LocationInfo locationInfo = await self.GetOrLoadInfo(selfRef, key);
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                locationInfo = await self.ClearExpiredLockIfNeeded(selfRef, key, locationInfo, "remove");
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                if (locationInfo == null)
                {
                    return;
                }

                if (expectedActorId != default && locationInfo.ActorId != expectedActorId)
                {
                    Log.Warning(
                        $"location remove skip by actor mismatch key: {key} expectedActorId: {expectedActorId} currentActorId: {locationInfo.ActorId}");
                    return;
                }

                if (IsLocked(locationInfo))
                {
                    if (expectedActorId != default)
                    {
                        Log.Warning(
                            $"location remove skip by active lock key: {key} expectedActorId: {expectedActorId} holdActorId: {locationInfo.ActorId} lockToken: {locationInfo.LockToken}");
                        return;
                    }

                    throw new RpcException(ErrorCode.ERR_LocationAlreadyLocked,
                        $"location remove rejected by lock key: {key} actorId: {locationInfo.ActorId} lockToken: {locationInfo.LockToken}");
                }

                LocationState oldState = self.CreateStateSnapshot(locationInfo);
                self.RemoveCachedInfo(key);

                try
                {
                    await self.RemoveStateFromDB(key);
                }
                catch
                {
                    self = selfRef;
                    if (self != null)
                    {
                        self.RestoreCachedState(key, oldState);
                    }

                    throw;
                }

                self = selfRef;
                if (self == null)
                {
                    return;
                }
                Log.Info($"location remove key: {key}");
            }
        }

        public static async ETTask<long> Lock(this LocationOneType self, long key, ActorId actorId, int time = 0)
        {
            EntityRef<LocationOneType> selfRef = self;
            long coroutineLockType = (self.Id << 32) | CoroutineLockType.Location;
            using (await self.Root().CoroutineLockComponent.Wait(coroutineLockType, key))
            {
                LocationInfo currentInfo = await self.GetOrLoadInfo(selfRef, key);
                self = selfRef;
                if (self == null)
                {
                    return default;
                }

                currentInfo = await self.ClearExpiredLockIfNeeded(selfRef, key, currentInfo, "lock");
                self = selfRef;
                if (self == null)
                {
                    return default;
                }

                if (IsLocked(currentInfo))
                {
                    if (currentInfo.ActorId == actorId)
                    {
                        Log.Warning(
                            $"location lock idempotent key: {key} instanceId: {actorId} lockToken: {currentInfo.LockToken}");
                        return currentInfo.LockToken;
                    }

                    throw new RpcException(ErrorCode.ERR_LocationAlreadyLocked,
                        $"location lock already exists key: {key} actorId: {currentInfo.ActorId} requestActorId: {actorId}");
                }

                if (currentInfo != null && currentInfo.ActorId != default && currentInfo.ActorId != actorId)
                {
                    throw new RpcException(ErrorCode.ERR_LocationLockOwnerMismatch,
                        $"location lock owner mismatch key: {key} requestActorId: {actorId} holdActorId: {currentInfo.ActorId}");
                }

                bool createdNewInfo = false;
                bool actorIdFilledByLock = false;
                ActorId oldActorId = default;
                if (currentInfo == null)
                {
                    createdNewInfo = true;
                    currentInfo = self.CreateInfo(key, actorId);
                    actorIdFilledByLock = true;
                }
                else
                {
                    oldActorId = currentInfo.ActorId;
                    if (currentInfo.ActorId == default && actorId != default)
                    {
                        // 兼容历史空路由记录：在加锁时补齐持有者，避免解锁时出现 owner mismatch。
                        currentInfo.ActorId = actorId;
                        actorIdFilledByLock = true;
                    }
                }

                long lockToken = IdGenerater.Instance.GenerateId();
                long lockExpireTime = time > 0 ? self.GetSingleton<TimeInfo>().ServerNow() + time : 0;
                currentInfo.LockToken = lockToken;
                currentInfo.LockExpireTime = lockExpireTime;
                EntityRef<LocationInfo> lockInfoRef = currentInfo;

                try
                {
                    await self.SaveStateToDB(currentInfo);
                }
                catch
                {
                    self = selfRef;
                    if (self != null)
                    {
                        currentInfo = lockInfoRef;
                        if (currentInfo != null)
                        {
                            currentInfo.LockToken = default;
                            currentInfo.LockExpireTime = default;
                            if (actorIdFilledByLock)
                            {
                                currentInfo.ActorId = oldActorId;
                            }
                            if (createdNewInfo)
                            {
                                self.RemoveCachedInfo(key);
                            }
                        }
                    }
                    throw;
                }

                self = selfRef;
                if (self == null)
                {
                    return default;
                }

                Log.Info(
                    $"location lock key: {key} instanceId: {actorId} lockToken: {lockToken} lockExpireTime: {lockExpireTime}");

                return lockToken;
            }
        }

        public static async ETTask UnLock(this LocationOneType self, long key, ActorId oldActorId, ActorId newInstanceId, long lockToken = 0)
        {
            EntityRef<LocationOneType> selfRef = self;
            long coroutineLockType = (self.Id << 32) | CoroutineLockType.Location;
            using (await self.Root().CoroutineLockComponent.Wait(coroutineLockType, key))
            {
                LocationInfo locationInfo = await self.GetOrLoadInfo(selfRef, key);
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                locationInfo = await self.ClearExpiredLockIfNeeded(selfRef, key, locationInfo, "unlock");
                self = selfRef;
                if (self == null)
                {
                    return;
                }

                if (!IsLocked(locationInfo))
                {
                    throw new RpcException(ErrorCode.ERR_LocationLockNotFound,
                        $"location unlock not found key: {key} oldActorId: {oldActorId}");
                }

                if (lockToken != 0 && locationInfo.LockToken != lockToken)
                {
                    throw new RpcException(ErrorCode.ERR_LocationLockTokenMismatch,
                        $"location unlock token mismatch key: {key} oldActorId: {oldActorId} requestToken: {lockToken} holdToken: {locationInfo.LockToken}");
                }

                if (oldActorId != locationInfo.ActorId)
                {
                    throw new RpcException(ErrorCode.ERR_LocationLockOwnerMismatch,
                        $"location unlock oldInstanceId is different key: {key} requestOldActorId: {oldActorId} holdActorId: {locationInfo.ActorId}");
                }

                Log.Info($"location unlock key: {key} instanceId: {oldActorId} newInstanceId: {newInstanceId} lockToken: {lockToken}");

                ActorId oldRouteActorId = locationInfo.ActorId;
                long oldLockToken = locationInfo.LockToken;
                long oldLockExpireTime = locationInfo.LockExpireTime;
                EntityRef<LocationInfo> locationInfoRef = locationInfo;

                locationInfo.ActorId = newInstanceId;
                locationInfo.LockToken = default;
                locationInfo.LockExpireTime = default;

                try
                {
                    await self.SaveStateToDB(locationInfo);
                }
                catch
                {
                    locationInfo = locationInfoRef;
                    if (locationInfo != null)
                    {
                        locationInfo.ActorId = oldRouteActorId;
                        locationInfo.LockToken = oldLockToken;
                        locationInfo.LockExpireTime = oldLockExpireTime;
                    }

                    throw;
                }
            }
        }

        public static async ETTask<ActorId> Get(this LocationOneType self, long key)
        {
            EntityRef<LocationOneType> selfRef = self;
            if (self.TryGetCachedInfo(key, out LocationInfo locationInfo))
            {
                if (IsLocked(locationInfo))
                {
                    if (!IsExpiredLock(locationInfo))
                    {
                        throw new RpcException(ErrorCode.ERR_LocationGetRetry,
                            $"location get retry key: {key} actorId: {locationInfo.ActorId} lockToken: {locationInfo.LockToken}");
                    }
                }
                else
                {
                    ActorId cachedActorId = locationInfo.ActorId;
                    Log.Info($"location get key: {key} actorId: {cachedActorId}");
                    return cachedActorId;
                }
            }

            long coroutineLockType = (self.Id << 32) | CoroutineLockType.Location;
            using (await self.Root().CoroutineLockComponent.Wait(coroutineLockType, key))
            {
                self = selfRef;
                if (self == null)
                {
                    return default;
                }

                if (!self.TryGetCachedInfo(key, out locationInfo))
                {
                    LocationState state = await self.LoadStateFromDB(key);
                    self = selfRef;
                    if (self == null)
                    {
                        return default;
                    }
                    locationInfo = self.ApplyLoadedStateIfNeeded(key, state);
                }

                locationInfo = await self.ClearExpiredLockIfNeeded(selfRef, key, locationInfo, "get");
                self = selfRef;
                if (self == null)
                {
                    return default;
                }

                if (IsLocked(locationInfo))
                {
                    throw new RpcException(ErrorCode.ERR_LocationGetRetry,
                        $"location get retry key: {key} actorId: {locationInfo.ActorId} lockToken: {locationInfo.LockToken}");
                }

                ActorId actorId = locationInfo == null ? default : locationInfo.ActorId;
                Log.Info($"location get key: {key} actorId: {actorId}");
                return actorId;
            }
        }
    }

    [EntitySystemOf(typeof(LocationManagerComponent))]
    public static partial class LocationComoponentSystem
    {
        [Event(SceneType.All)]
        public class OnServiceChangeAddService_RefreshLocationPrimary: AEvent<Scene, OnServiceChangeAddService>
        {
            protected override async ETTask Run(Scene scene, OnServiceChangeAddService args)
            {
                LocationManagerComponent locationManagerComponent = scene.GetComponent<LocationManagerComponent>();
                if (locationManagerComponent != null)
                {
                    locationManagerComponent.RefreshPrimaryState();
                }

                await ETTask.CompletedTask;
            }
        }

        [Event(SceneType.All)]
        public class OnServiceChangeRemoveService_RefreshLocationPrimary: AEvent<Scene, OnServiceChangeRemoveService>
        {
            protected override async ETTask Run(Scene scene, OnServiceChangeRemoveService args)
            {
                LocationManagerComponent locationManagerComponent = scene.GetComponent<LocationManagerComponent>();
                if (locationManagerComponent != null)
                {
                    locationManagerComponent.RefreshPrimaryState();
                }

                await ETTask.CompletedTask;
            }
        }

        [EntitySystem]
        private static void Awake(this LocationManagerComponent self)
        {
            self.IsPrimaryLocation = false;
            self.PrimaryLocationSceneName = string.Empty;
        }

        private static void ClearCache(this LocationOneType self)
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

        public static void ClearAllCache(this LocationManagerComponent self)
        {
            foreach ((long _, Entity value) in self.Children)
            {
                LocationOneType locationOneType = (LocationOneType)value;
                locationOneType.ClearCache();
            }
        }

        public static void RefreshPrimaryState(this LocationManagerComponent self)
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
                Log.Warning($"location manager demoted to backup scene: {root.Name} primary: {primarySceneName}");
                return;
            }

            if (oldIsPrimaryLocation != isPrimaryLocation || !string.Equals(oldPrimarySceneName, primarySceneName, StringComparison.Ordinal))
            {
                Log.Info($"location manager primary state changed scene: {root.Name} isPrimary: {isPrimaryLocation} primary: {primarySceneName}");
            }
        }

        public static LocationOneType Get(this LocationManagerComponent self, int locationType)
        {
            LocationOneType locationOneType = self.GetChild<LocationOneType>(locationType);
            if (locationOneType != null)
            {
                return locationOneType;
            }
            locationOneType = self.AddChildWithId<LocationOneType>(locationType);
            return locationOneType;
        }

        public static bool EnsurePrimary(this LocationManagerComponent self, IResponse response)
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
