using System;
using System.Collections.Generic;
using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    internal static class Actorlocation_TestHelper
    {
        public static ActorId CreateActorId(int fiberId, int instanceId)
        {
            Address address = EnsureAddressReady();
            return new ActorId(address, new FiberInstanceId(fiberId, instanceId));
        }

        public static Scene PrepareLocationScene(Fiber testFiber)
        {
            if (testFiber == null)
            {
                throw new Exception("test fiber is null");
            }

            Scene scene = testFiber.Root;
            if (scene == null)
            {
                throw new Exception("test root scene is null");
            }

            _ = scene.TimerComponent ?? scene.AddComponent<TimerComponent>();
            _ = scene.CoroutineLockComponent ?? scene.AddComponent<CoroutineLockComponent>();
            _ = scene.GetComponent<DBManagerComponent>() ?? scene.AddComponent<DBManagerComponent>();
            TestFiberDatabaseCleanupComponent cleanupComponent =
                    scene.GetComponent<TestFiberDatabaseCleanupComponent>() ?? scene.AddComponent<TestFiberDatabaseCleanupComponent>();
            cleanupComponent.LogicalDbNames.Add(LocationPersistenceConst.DBName);
            _ = scene.GetComponent<LocationManagerComponent>() ?? scene.AddComponent<LocationManagerComponent>();

            return scene;
        }

        public static Scene PrepareProxyScene(Fiber testFiber)
        {
            Scene scene = PrepareLocationScene(testFiber);
            _ = scene.GetComponent<MailBoxComponent>() ?? scene.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            _ = scene.GetComponent<ProcessInnerSender>() ?? scene.AddComponent<ProcessInnerSender>();
            _ = scene.GetComponent<MessageSender>() ?? scene.AddComponent<MessageSender>();

            ServiceDiscoveryProxy serviceDiscoveryProxy =
                    scene.GetComponent<ServiceDiscoveryProxy>() ?? scene.AddComponent<ServiceDiscoveryProxy>();
            serviceDiscoveryProxy.RemoveComponent<ServiceDiscoveryProxyHeartbeat>();

            _ = scene.GetComponent<LocationProxyComponent>() ?? scene.AddComponent<LocationProxyComponent>();
            _ = scene.GetComponent<MessageLocationSenderComponent>() ?? scene.AddComponent<MessageLocationSenderComponent>();

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
            if (proxy == null)
            {
                throw new Exception("service discovery proxy is null");
            }

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

        public static Address EnsureAddressSingletonReady()
        {
            return EnsureAddressReady();
        }

        public static LocationOneType GetLocationOneType(Scene scene, int locationType)
        {
            if (scene == null)
            {
                throw new Exception("scene is null");
            }

            LocationManagerComponent locationManagerComponent = scene.GetComponent<LocationManagerComponent>();
            if (locationManagerComponent == null)
            {
                throw new Exception($"location manager component not found in scene: {scene.Name}");
            }

            return locationManagerComponent.Get(locationType);
        }

        public static LocationOneType EnsureLocation(EntityRef<LocationOneType> locationRef, string scenario)
        {
            LocationOneType location = locationRef;
            if (location == null)
            {
                throw new Exception($"{scenario}: location disposed");
            }

            return location;
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

        public static async ETTask ExpectRpcError(Func<ETTask> action, int expectedError, string scenario)
        {
            try
            {
                await action();
                throw new Exception($"{scenario}: expected RpcException({expectedError}), but no exception");
            }
            catch (RpcException e)
            {
                if (e.Error != expectedError)
                {
                    throw new Exception($"{scenario}: expected RpcException({expectedError}), actual RpcException({e.Error})");
                }
            }
        }

        public static async ETTask WaitUntil(
            TimerComponent timerComponent,
            Func<bool> predicate,
            int timeoutMs,
            int intervalMs,
            string scenario)
        {
            EntityRef<TimerComponent> timerRef = timerComponent;
            long deadline = TimeInfo.Instance.ServerNow() + timeoutMs;
            while (TimeInfo.Instance.ServerNow() <= deadline)
            {
                if (predicate())
                {
                    return;
                }

                timerComponent = timerRef;
                if (timerComponent == null)
                {
                    throw new Exception($"{scenario}: timer disposed");
                }

                await timerComponent.WaitAsync(intervalMs);
            }

            throw new Exception($"{scenario}: timeout in {timeoutMs}ms");
        }

        private static Address EnsureAddressReady()
        {
            AddressSingleton addressSingleton = AddressSingleton.Instance;
            if (addressSingleton == null)
            {
                addressSingleton = World.Instance.AddSingleton<AddressSingleton>();
            }

            if (string.IsNullOrEmpty(addressSingleton.InnerIP)
                || string.IsNullOrEmpty(addressSingleton.OuterIP)
                || addressSingleton.InnerPort <= 0)
            {
                StartProcessConfig processConfig = World.Instance.GetSingleton<StartProcessConfigCategory>().Get(Options.Instance.Process);
                StartMachineConfig startMachineConfig =
                        World.Instance.GetSingleton<StartMachineConfigCategory>()?.Get(processConfig.MachineId);
                addressSingleton.InnerIP ??= startMachineConfig?.InnerIP;
                addressSingleton.OuterIP ??= startMachineConfig?.OuterIP;
                addressSingleton.InnerPort = addressSingleton.InnerPort > 0 ? addressSingleton.InnerPort : processConfig.Port;
            }

            return addressSingleton.InnerAddress;
        }
    }
}
