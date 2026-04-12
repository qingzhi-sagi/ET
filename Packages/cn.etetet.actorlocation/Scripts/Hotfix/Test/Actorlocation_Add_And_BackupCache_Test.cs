using System;
using ET.Server;

namespace ET.Test
{
    public class Actorlocation_Add_ProxyReplaceExisting_Test : ATestHandler
    {
        private const int LocationType = 910011;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_Add_ProxyReplaceExisting_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareProxyScene(scope.TestFiber);
                _ = Actorlocation_TestHelper.EnsureAddressSingletonReady(scope.TestFiber);

                ServiceDiscoveryProxy serviceDiscoveryProxy = scene.GetComponent<ServiceDiscoveryProxy>();
                LocationProxyComponent locationProxy = scene.GetComponent<LocationProxyComponent>();
                LocationOneType location = Actorlocation_TestHelper.GetLocationOneType(scene, LocationType);

                EntityRef<LocationProxyComponent> locationProxyRef = locationProxy;
                EntityRef<LocationOneType> locationRef = location;

                ActorId serviceActorId = scene.GetActorId();
                int zone = scene.Zone();
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, "a_add_exists_mock", serviceActorId, zone);

                long key = IdGenerater.Instance.GenerateId();
                ActorId actorA = Actorlocation_TestHelper.CreateActorId(301020, 1);
                ActorId actorB = Actorlocation_TestHelper.CreateActorId(301021, 1);

                await locationProxy.Add(LocationType, key, actorA);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("add-exists: location proxy disposed after first add");
                }

                LocationOneType currentLocation = Actorlocation_TestHelper.EnsureLocation(locationRef, "add-exists/direct-add-second");
                await currentLocation.Add(key, actorB);

                LocationProxyComponent currentProxy = locationProxyRef;
                if (currentProxy == null)
                {
                    throw new Exception("add-exists: location proxy disposed before second proxy add");
                }

                await currentProxy.Add(LocationType, key, actorB);

                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("add-exists: location proxy disposed before final get");
                }

                ActorId finalActorId = await locationProxy.Get(LocationType, key);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("add-exists: location proxy disposed after final get");
                }

                Actorlocation_TestHelper.AssertActorEqual(actorB, finalActorId, "add-exists/final-actor");

                await locationProxy.Remove(LocationType, key);

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Error($"Actorlocation_Add_ProxyReplaceExisting_Test failed: {e}");
                return 1;
            }
        }
    }

    public class Actorlocation_BecomeBackup_ClearCache_Test : ATestHandler
    {
        private const int LocationType = 910012;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_BecomeBackup_ClearCache_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareProxyScene(scope.TestFiber);
                ServiceDiscoveryProxy serviceDiscoveryProxy = scene.GetComponent<ServiceDiscoveryProxy>();
                LocationManagerComponent locationManagerComponent = scene.GetComponent<LocationManagerComponent>();
                LocationOneType location = Actorlocation_TestHelper.GetLocationOneType(scene, LocationType);
                TimerComponent timerComponent = scene.TimerComponent;

                EntityRef<ServiceDiscoveryProxy> serviceDiscoveryProxyRef = serviceDiscoveryProxy;
                EntityRef<LocationManagerComponent> locationManagerRef = locationManagerComponent;
                EntityRef<LocationOneType> locationRef = location;
                EntityRef<TimerComponent> timerRef = timerComponent;

                ActorId selfActorId = scene.GetActorId();
                int zone = scene.Zone();
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, scene.Name, selfActorId, zone, 200);

                await Actorlocation_TestHelper.WaitUntil(
                    timerComponent,
                    () =>
                    {
                        LocationManagerComponent current = locationManagerRef;
                        return current != null && current.IsPrimaryLocation;
                    },
                    1000,
                    10,
                    "become-backup/become-primary");

                long key = IdGenerater.Instance.GenerateId();
                ActorId routeActorId = Actorlocation_TestHelper.CreateActorId(301022, 1);

                await location.Add(key, routeActorId);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "become-backup/add-route");
                Actorlocation_TestHelper.AssertTrue(location.GetChild<LocationInfo>(key) != null, "become-backup/cache-before-demote");

                timerComponent = timerRef;
                if (timerComponent == null)
                {
                    throw new Exception("become-backup: timer disposed before demote");
                }

                serviceDiscoveryProxy = serviceDiscoveryProxyRef;
                if (serviceDiscoveryProxy == null)
                {
                    throw new Exception("become-backup: service discovery proxy disposed before demote");
                }

                Actorlocation_TestHelper.AddLocalLocationService(
                    serviceDiscoveryProxy,
                    "0_location_primary",
                    Actorlocation_TestHelper.CreateActorId(301023, 1),
                    zone,
                    100);

                await Actorlocation_TestHelper.WaitUntil(
                    timerComponent,
                    () =>
                    {
                        LocationManagerComponent currentManager = locationManagerRef;
                        LocationOneType currentLocation = locationRef;
                        return currentManager != null
                               && currentLocation != null
                               && !currentManager.IsPrimaryLocation
                               && currentManager.PrimaryLocationSceneName == "0_location_primary"
                               && currentLocation.GetChild<LocationInfo>(key) == null;
                    },
                    1000,
                    10,
                    "become-backup/cache-cleared-after-demote");

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Error($"Actorlocation_BecomeBackup_ClearCache_Test failed: {e}");
                return 1;
            }
        }
    }

    public class Actorlocation_BackupRejectsRequests_Test : ATestHandler
    {
        private const int LocationType = 910013;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_BackupRejectsRequests_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareProxyScene(scope.TestFiber);
                ServiceDiscoveryProxy serviceDiscoveryProxy = scene.GetComponent<ServiceDiscoveryProxy>();
                LocationManagerComponent locationManagerComponent = scene.GetComponent<LocationManagerComponent>();
                TimerComponent timerComponent = scene.TimerComponent;
                MessageSender messageSender = scene.GetComponent<MessageSender>();

                EntityRef<LocationManagerComponent> locationManagerRef = locationManagerComponent;
                EntityRef<TimerComponent> timerRef = timerComponent;

                ActorId selfActorId = scene.GetActorId();
                int zone = scene.Zone();
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, scene.Name, selfActorId, zone, 200);
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, "0_location_primary",
                    Actorlocation_TestHelper.CreateActorId(301024, 1), zone, 100);

                await Actorlocation_TestHelper.WaitUntil(
                    timerComponent,
                    () =>
                    {
                        LocationManagerComponent current = locationManagerRef;
                        return current != null
                               && !current.IsPrimaryLocation
                               && current.PrimaryLocationSceneName == "0_location_primary";
                    },
                    1000,
                    10,
                    "backup-rejects/become-backup");

                using ObjectGetRequest request = ObjectGetRequest.Create();
                request.Type = LocationType;
                request.Key = IdGenerater.Instance.GenerateId();

                using ObjectGetResponse response = await messageSender.Call(selfActorId, request) as ObjectGetResponse;

                timerComponent = timerRef;
                if (timerComponent == null)
                {
                    throw new Exception("backup-rejects: timer disposed after call");
                }

                Actorlocation_TestHelper.AssertTrue(response != null, "backup-rejects/response-null");
                Actorlocation_TestHelper.AssertEqual(ErrorCode.ERR_LocationFollowerRejected, response.Error,
                    "backup-rejects/error-code");

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Error($"Actorlocation_BackupRejectsRequests_Test failed: {e}");
                return 1;
            }
        }
    }
}
