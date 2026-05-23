using System;
using System.Collections.Generic;
using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    internal static class Actorlocation_TestHelper
    {
        public static ActorId CreateActorId(Fiber fiber, long fiberId, long instanceId)
        {
            Address address = fiber.GetSingleton<AddressSingleton>().InnerAddress;
            return new ActorId(address, new FiberInstanceId(fiberId, instanceId));
        }

        public static Scene PrepareLocationScene(Fiber testFiber)
        {
            Scene scene = testFiber.Root;

            scene.AddComponent<TimerComponent>();
            scene.AddComponent<CoroutineLockComponent>();
            scene.AddComponent<DBManagerComponent>();
            scene.GetComponent<TestFiberDatabaseCleanupComponent>().RegisterLogicalDbName(LocationPersistenceConst.DBName);
            scene.AddComponent<LocationComponent>();

            return scene;
        }

        public static Scene PrepareProxyScene(Fiber testFiber)
        {
            Scene scene = PrepareLocationScene(testFiber);
            scene.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            scene.AddComponent<ProcessInnerSender>();
            scene.AddComponent<MessageSender>();
            ServiceDiscoveryProxy serviceDiscoveryProxy = scene.AddComponent<ServiceDiscoveryProxy>();

            serviceDiscoveryProxy.RemoveComponent<ServiceDiscoveryProxyHeartbeat>();

            scene.AddComponent<LocationProxyComponent>();
            scene.AddComponent<MessageLocationSenderComponent>();

            return scene;
        }

        public static void AddLocalLocationService(ServiceDiscoveryProxy proxy, string sceneName, ActorId actorId, int zone,
            long? priorityId = null)
        {
            NotifyLocalLocationService(proxy, sceneName, actorId, zone, 1, priorityId);
        }

        public static void RemoveLocalLocationService(ServiceDiscoveryProxy proxy, string sceneName, ActorId actorId, int zone,
            long? priorityId = null)
        {
            NotifyLocalLocationService(proxy, sceneName, actorId, zone, 2, priorityId);
        }

        private static void NotifyLocalLocationService(ServiceDiscoveryProxy proxy, string sceneName, ActorId actorId, int zone, int changeType,
            long? priorityId)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new Exception("scene name is empty");
            }

            using ServiceInfoProto serviceInfoProto = ServiceInfoProto.Create();
            serviceInfoProto.SceneName = sceneName;
            serviceInfoProto.ActorId = actorId;
            serviceInfoProto.Metadata.Add(ServiceMetaKey.SceneType, SceneTypeSingleton.Instance.GetSceneName(SceneType.Location));
            if (changeType == 1 && !priorityId.HasValue)
            {
                priorityId = IdGenerater.Instance.GenerateId();
            }

            if (priorityId.HasValue)
            {
                serviceInfoProto.Metadata.Add(ServiceMetaKey.PriorityId, priorityId.Value.ToString());
            }

            proxy.OnServiceChangeNotification(changeType, new List<ServiceInfoProto> { serviceInfoProto });
        }

        public static LocationComponent GetLocationComponent(Scene scene)
        {
            return scene.GetComponent<LocationComponent>();
        }

        public static LocationComponent EnsureLocationComponent(EntityRef<LocationComponent> locationComponentRef, string scenario)
        {
            LocationComponent locationComponent = locationComponentRef;
            return locationComponent ?? throw new Exception($"{scenario}: location component disposed");
        }

        public static LocationInfo GetLocationInfo(LocationComponent locationComponent, long key)
        {
            return locationComponent.GetChild<LocationInfo>(key);
        }

        public static LocationTypeState GetLocationTypeState(LocationComponent locationComponent, int locationType, long key,
        string scenario)
        {
            LocationInfo locationInfo = GetLocationInfo(locationComponent, key);
            AssertTrue(locationInfo != null, $"{scenario}: location info missing");
            AssertTrue(locationInfo.TypeStates.TryGetValue(locationType, out LocationTypeState state), $"{scenario}: route state missing");
            return state;
        }

        public static void AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }

        public static void AssertEqual<T>(T expected, T actual, string scenario)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new Exception($"{scenario}: expected={expected}, actual={actual}");
            }
        }

        public static void AssertActorEqual(ActorId expected, ActorId actual, string scenario)
        {
            if (expected != actual)
            {
                throw new Exception($"{scenario}: expected={expected}, actual={actual}");
            }
        }

        public static void AssertRpcError(RpcException exception, int expectedError, string scenario)
        {
            if (exception == null)
            {
                throw new Exception($"{scenario}: expected RpcException({expectedError}), but exception is null");
            }

            if (exception.Error != expectedError)
            {
                throw new Exception($"{scenario}: expected RpcException({expectedError}), actual RpcException({exception.Error})");
            }
        }

    }
}
