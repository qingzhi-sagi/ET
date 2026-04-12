using System;
using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(LocationProxyComponent))]
    public static partial class LocationProxyComponentSystem
    {
        [Event(SceneType.All)]
        public class OnServiceChangeRemoveService_ClearLocationPrimary: AEvent<Scene, OnServiceChangeRemoveService>
        {
            protected override async ETTask Run(Scene scene, OnServiceChangeRemoveService args)
            {
                LocationProxyComponent locationProxyComponent = scene.GetComponent<LocationProxyComponent>();
                if (locationProxyComponent == null)
                {
                    await ETTask.CompletedTask;
                    return;
                }

                if (locationProxyComponent.primaryLocationSceneName != args.ServiceName)
                {
                    await ETTask.CompletedTask;
                    return;
                }

                try
                {
                    StartSceneConfigCategory startSceneConfigCategory = scene.Fiber().GetSingleton<StartSceneConfigCategory>();
                    StartSceneConfig startSceneConfig = startSceneConfigCategory.GetBySceneName(args.ServiceName);
                    string locationSceneType = SceneTypeSingleton.Instance.GetSceneName(SceneType.Location);
                    if (startSceneConfig.SceneType != locationSceneType)
                    {
                        await ETTask.CompletedTask;
                        return;
                    }
                }
                catch
                {
                    await ETTask.CompletedTask;
                    return;
                }

                string oldPrimarySceneName = locationProxyComponent.primaryLocationSceneName;
                ulong oldPriorityId = locationProxyComponent.primaryLocationPriorityId;
                locationProxyComponent.primaryLocationSceneName = string.Empty;
                locationProxyComponent.primaryLocationPriorityId = 0;
                Log.Warning(
                    $"location proxy primary removed by service change: {oldPrimarySceneName} priorityId: {oldPriorityId}, scene: {scene.Name}");

                await ETTask.CompletedTask;
            }
        }

        [EntitySystem]
        private static void Awake(this LocationProxyComponent self)
        {
            self.primaryLocationPriorityId = 0;
            self.locationRequestRetryTimes = 20;
            self.locationRequestRetryIntervalMs = 100;
        }

        private static string GetPrimaryLocationSceneName(this LocationProxyComponent self)
        {
            ServiceDiscoveryProxy serviceDiscoveryProxy = self.Root().GetComponent<ServiceDiscoveryProxy>();
            List<ServiceInfo> serviceInfos = serviceDiscoveryProxy.GetBySceneType(SceneType.Location);
            if (serviceInfos.Count == 0)
            {
                throw new RpcException(ErrorCode.ERR_LocationPrimaryUnavailable, $"not found location scene in zone: {self.Zone()}");
            }

            ServiceInfo stablePrimaryServiceInfo = LocationPrimaryHelper.SelectStablePrimaryServiceInfo(serviceInfos);
            string stablePrimarySceneName = stablePrimaryServiceInfo.SceneName;
            ulong stablePrimaryPriorityId = LocationPrimaryHelper.GetPriorityId(stablePrimaryServiceInfo);
            string primarySceneName = self.primaryLocationSceneName;
            ulong primaryPriorityId = self.primaryLocationPriorityId;
            if (primarySceneName == stablePrimarySceneName && primaryPriorityId == stablePrimaryPriorityId)
            {
                return stablePrimarySceneName;
            }

            self.primaryLocationSceneName = stablePrimarySceneName;
            self.primaryLocationPriorityId = stablePrimaryPriorityId;
            Log.Info(
                $"location proxy primary switch: {primarySceneName}({primaryPriorityId}) -> {stablePrimarySceneName}({stablePrimaryPriorityId}) scene: {self.Root().Name}");
            return stablePrimarySceneName;
        }

        private static void ClearPrimary(this LocationProxyComponent self, string reason)
        {
            if (self == null)
            {
                return;
            }

            string oldPrimarySceneName = self.primaryLocationSceneName;
            ulong oldPriorityId = self.primaryLocationPriorityId;
            self.primaryLocationSceneName = string.Empty;
            self.primaryLocationPriorityId = 0;
            Log.Warning(
                $"location proxy clear primary reason: {reason} old: {oldPrimarySceneName}({oldPriorityId}) scene: {self.Root().Name}");
        }

        private static async ETTask<IResponse> CallPrimaryWithRetry(this LocationProxyComponent self, long key, IRequest request)
        {
            EntityRef<LocationProxyComponent> selfRef = self;
            int retryCount = 0;
            while (true)
            {
                self = selfRef;
                if (self == null)
                {
                    throw new Exception("location proxy disposed");
                }

                string primarySceneName = string.Empty;
                ulong primaryPriorityId = 0;
                try
                {
                    primarySceneName = self.GetPrimaryLocationSceneName();
                    primaryPriorityId = self.primaryLocationPriorityId;
                    ServiceDiscoveryProxy serviceDiscoveryProxy = self.Root().GetComponent<ServiceDiscoveryProxy>();
                    ActorId primaryActorId = serviceDiscoveryProxy.GetServiceInfo(primarySceneName).ActorId;
                    if (primaryActorId == default)
                    {
                        throw new Exception($"location primary actor id is empty: {primarySceneName}");
                    }

                    MessageSender messageSender = self.Root().GetComponent<MessageSender>();
                    if (messageSender == null)
                    {
                        throw new Exception($"location proxy message sender is null: {self.Root().Name}");
                    }

                    IResponse response = await messageSender.Call(primaryActorId, request);
                    if (response.Error == ErrorCode.ERR_LocationFollowerRejected
                        || response.Error == ErrorCode.ERR_LocationPrimaryUnavailable)
                    {
                        ++retryCount;
                        self = selfRef;
                        if (self == null)
                        {
                            return response;
                        }

                        self.ClearPrimary($"response-error:{response.Error}");
                        Log.Warning(
                            $"location proxy retry after primary rejection key: {key} scene: {primarySceneName} priorityId: {primaryPriorityId} retry: {retryCount}/{self.locationRequestRetryTimes} error: {response.Error} message: {response.Message}");

                        if (retryCount >= self.locationRequestRetryTimes)
                        {
                            return response;
                        }

                        Scene root = self.Root();
                        EntityRef<Scene> rootRef = root;
                        await root.TimerComponent.WaitAsync(self.locationRequestRetryIntervalMs);

                        root = rootRef;
                        if (root == null)
                        {
                            throw new Exception("location proxy root disposed");
                        }

                        continue;
                    }

                    return response;
                }
                catch (Exception e)
                {
                    ++retryCount;
                    self = selfRef;
                    if (self == null)
                    {
                        throw;
                    }

                    if (e is RpcException rpcException && rpcException.Error == ErrorCode.ERR_LocationPrimaryUnavailable)
                    {
                        self.ClearPrimary($"select-primary:{rpcException.Error}");
                    }

                    Log.Warning(
                        $"location proxy call failed key: {key} scene: {primarySceneName} priorityId: {primaryPriorityId} retry: {retryCount}/{self.locationRequestRetryTimes} error: {e.Message}");

                    if (retryCount >= self.locationRequestRetryTimes)
                    {
                        throw;
                    }

                    Scene root = self.Root();
                    EntityRef<Scene> rootRef = root;
                    await root.TimerComponent.WaitAsync(self.locationRequestRetryIntervalMs);

                    root = rootRef;
                    if (root == null)
                    {
                        throw new Exception("location proxy root disposed");
                    }
                }
            }
        }

        public static async ETTask Add(this LocationProxyComponent self, int type, long key, ActorId actorId)
        {
            Log.Info($"location proxy add {key}, {actorId} {self.GetSingleton<TimeInfo>().ServerNow()}");
            ObjectAddRequest request = ObjectAddRequest.Create();
            request.Type = type;
            request.Key = key;
            request.ActorId = actorId;

            ObjectAddResponse response = (ObjectAddResponse)await self.CallPrimaryWithRetry(key, request);
            if (response.Error != ErrorCode.ERR_Success)
            {
                throw new RpcException(response.Error,
                    $"location add failed key: {key} actorId: {actorId} error: {response.Message}");
            }
        }

        public static async ETTask<LocationLockTokenInfo> LockWithToken(this LocationProxyComponent self, int type, long key, ActorId actorId,
            int time = 60000)
        {
            Log.Info($"location proxy lock {key}, {actorId} {self.GetSingleton<TimeInfo>().ServerNow()}");

            ObjectLockRequest request = ObjectLockRequest.Create();
            request.Type = type;
            request.Key = key;
            request.ActorId = actorId;
            request.Time = time;

            ObjectLockResponse response = (ObjectLockResponse)await self.CallPrimaryWithRetry(key, request);
            if (response.Error != ErrorCode.ERR_Success)
            {
                throw new RpcException(response.Error,
                    $"location lock failed key: {key} actorId: {actorId} error: {response.Message}");
            }

            return new LocationLockTokenInfo()
            {
                LockToken = response.LockToken,
            };
        }

        public static async ETTask Lock(this LocationProxyComponent self, int type, long key, ActorId actorId,
            int time = 60000)
        {
            await self.LockWithToken(type, key, actorId, time);
        }

        public static async ETTask UnLock(this LocationProxyComponent self, int type, long key, ActorId oldActorId,
            ActorId newActorId)
        {
            await self.UnLock(type, key, oldActorId, newActorId, 0);
        }

        public static async ETTask UnLock(this LocationProxyComponent self, int type, long key, ActorId oldActorId,
            ActorId newActorId, long lockToken)
        {
            Log.Info(
                $"location proxy unlock {key}, {newActorId} lockToken: {lockToken} {self.GetSingleton<TimeInfo>().ServerNow()}");

            ObjectUnLockRequest request = ObjectUnLockRequest.Create();
            request.Type = type;
            request.Key = key;
            request.OldActorId = oldActorId;
            request.NewActorId = newActorId;
            request.LockToken = lockToken;

            ObjectUnLockResponse response = (ObjectUnLockResponse)await self.CallPrimaryWithRetry(key, request);
            if (response.Error != ErrorCode.ERR_Success)
            {
                throw new RpcException(response.Error,
                    $"location unlock failed key: {key} oldActorId: {oldActorId} newActorId: {newActorId} lockToken: {lockToken} error: {response.Message}");
            }
        }

        public static async ETTask UnLockWithRetry(this LocationProxyComponent self, int type, long key, ActorId oldActorId,
            ActorId newActorId, long lockToken)
        {
            EntityRef<LocationProxyComponent> selfRef = self;
            int retryCount = 0;
            while (true)
            {
                self = selfRef;
                if (self == null)
                {
                    throw new Exception("location proxy disposed");
                }

                try
                {
                    await self.UnLock(type, key, oldActorId, newActorId, lockToken);
                    return;
                }
                catch (RpcException e) when (e.Error == ErrorCode.ERR_LocationLockNotFound
                                             || e.Error == ErrorCode.ERR_LocationLockOwnerMismatch
                                             || e.Error == ErrorCode.ERR_LocationLockTokenMismatch)
                {
                    LocationProxyComponent selfForGet = selfRef;
                    if (selfForGet == null)
                    {
                        throw new Exception("location proxy disposed");
                    }
                    ActorId actorId = await selfForGet.Get(type, key);

                    self = selfRef;
                    if (self == null)
                    {
                        throw new Exception("location proxy disposed");
                    }

                    if (actorId == newActorId)
                    {
                        Log.Warning(
                            $"location proxy unlock compensate success key: {key} actorId: {actorId} lockToken: {lockToken} reason: {e.Error}");
                        return;
                    }

                    ++retryCount;
                    Log.Warning(
                        $"location proxy unlock compensate retry key: {key} actorId: {actorId} target: {newActorId} retry: {retryCount}/{self.locationRequestRetryTimes} lockToken: {lockToken} reason: {e.Error}");
                    if (retryCount >= self.locationRequestRetryTimes)
                    {
                        throw;
                    }

                    Scene root = self.Root();
                    EntityRef<Scene> rootRef = root;
                    await root.TimerComponent.WaitAsync(self.locationRequestRetryIntervalMs * retryCount);
                    root = rootRef;
                    if (root == null)
                    {
                        throw new Exception("location proxy root disposed");
                    }
                }
            }
        }

        public static async ETTask Remove(this LocationProxyComponent self, int type, long key)
        {
            await self.Remove(type, key, default);
        }

        public static async ETTask Remove(this LocationProxyComponent self, int type, long key, ActorId expectedActorId)
        {
            Log.Info($"location proxy remove {key}, {self.GetSingleton<TimeInfo>().ServerNow()}");

            ObjectRemoveRequest request = ObjectRemoveRequest.Create();
            request.Type = type;
            request.Key = key;
            request.ExpectedActorId = expectedActorId;

            ObjectRemoveResponse response = (ObjectRemoveResponse)await self.CallPrimaryWithRetry(key, request);
            if (response.Error != ErrorCode.ERR_Success)
            {
                throw new RpcException(response.Error,
                    $"location remove failed key: {key} expectedActorId: {expectedActorId} error: {response.Message}");
            }
        }

        public static async ETTask<ActorId> Get(this LocationProxyComponent self, int type, long key)
        {
            if (key == 0)
            {
                throw new Exception("get location key 0");
            }

            EntityRef<LocationProxyComponent> selfRef = self;
            int retryCount = 0;
            while (true)
            {
                self = selfRef;
                if (self == null)
                {
                    throw new Exception("location proxy disposed");
                }

                ObjectGetRequest request = ObjectGetRequest.Create();
                request.Type = type;
                request.Key = key;

                ObjectGetResponse response = (ObjectGetResponse)await self.CallPrimaryWithRetry(key, request);
                if (response.Error == ErrorCode.ERR_Success)
                {
                    return response.ActorId;
                }

                if (response.Error == ErrorCode.ERR_LocationGetRetry)
                {
                    ++retryCount;
                    self = selfRef;
                    if (self == null)
                    {
                        throw new Exception("location proxy disposed");
                    }

                    if (retryCount >= self.locationRequestRetryTimes)
                    {
                        throw new RpcException(response.Error,
                            $"location get retry exceeded key: {key} retry: {retryCount}/{self.locationRequestRetryTimes}");
                    }

                    int delayMs = self.locationRequestRetryIntervalMs * retryCount;
                    if (delayMs <= 0)
                    {
                        delayMs = 1;
                    }

                    Scene root = self.Root();
                    EntityRef<Scene> rootRef = root;
                    await root.TimerComponent.WaitAsync(delayMs);
                    root = rootRef;
                    if (root == null)
                    {
                        throw new Exception("location proxy root disposed");
                    }
                    continue;
                }

                throw new RpcException(response.Error,
                    $"location get failed key: {key} error: {response.Error} message: {response.Message}");
            }
        }

        public static async ETTask AddLocation(this Entity self, int type)
        {
            await self.Root().GetComponent<LocationProxyComponent>().Add(type, self.Id, self.GetActorId());
        }

        public static async ETTask RemoveLocation(this Entity self, int type)
        {
            await self.Root().GetComponent<LocationProxyComponent>().Remove(type, self.Id, self.GetActorId());
        }
    }
}
