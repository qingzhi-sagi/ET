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

                ServiceDiscoveryProxy serviceDiscoveryProxy = scene.GetComponent<ServiceDiscoveryProxy>();
                LocationProxyComponent locationProxy = scene.GetComponent<LocationProxyComponent>();
                LocationComponent location = Actorlocation_TestHelper.GetLocationComponent(scene);

                EntityRef<LocationProxyComponent> locationProxyRef = locationProxy;
                EntityRef<LocationComponent> locationRef = location;

                ActorId serviceActorId = scene.GetActorId();
                int zone = scene.Zone();
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, "a_add_exists_mock", serviceActorId, zone);

                long key = IdGenerater.Instance.GenerateId();
                ActorId actorA = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 301020, 1);
                ActorId actorB = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 301021, 1);

                await locationProxy.Add(LocationType, key, actorA);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("add-exists: location proxy disposed after first add");
                }

                LocationComponent currentLocation = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "add-exists/direct-add-second");
                await currentLocation.Add(LocationType, key, actorB);

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
                LocationComponent locationComponent = scene.GetComponent<LocationComponent>();
                LocationComponent location = Actorlocation_TestHelper.GetLocationComponent(scene);

                EntityRef<ServiceDiscoveryProxy> serviceDiscoveryProxyRef = serviceDiscoveryProxy;
                EntityRef<LocationComponent> locationComponentRef = locationComponent;
                EntityRef<LocationComponent> locationRef = location;

                ActorId selfActorId = scene.GetActorId();
                int zone = scene.Zone();
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, scene.Name, selfActorId, zone, 200);
                locationComponent.RefreshPrimaryState();
                Actorlocation_TestHelper.AssertTrue(locationComponent.IsPrimaryLocation, "become-backup/become-primary");

                long key = IdGenerater.Instance.GenerateId();
                ActorId routeActorId = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 301022, 1);

                await location.Add(LocationType, key, routeActorId);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "become-backup/add-route");
                Actorlocation_TestHelper.AssertTrue(Actorlocation_TestHelper.GetLocationInfo(location, key) != null, "become-backup/cache-before-demote");

                serviceDiscoveryProxy = serviceDiscoveryProxyRef;
                if (serviceDiscoveryProxy == null)
                {
                    throw new Exception("become-backup: service discovery proxy disposed before demote");
                }

                Actorlocation_TestHelper.AddLocalLocationService(
                    serviceDiscoveryProxy,
                    "0_location_primary",
                    Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 301023, 1),
                    zone,
                    100);
                locationComponent = locationComponentRef;
                if (locationComponent == null)
                {
                    throw new Exception("become-backup: location component disposed before refresh");
                }
                locationComponent.RefreshPrimaryState();
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "become-backup/cache-cleared-after-demote");
                Actorlocation_TestHelper.AssertTrue(!locationComponent.IsPrimaryLocation, "become-backup/demoted");
                Actorlocation_TestHelper.AssertEqual("0_location_primary", locationComponent.PrimaryLocationSceneName,
                    "become-backup/new-primary");
                Actorlocation_TestHelper.AssertTrue(Actorlocation_TestHelper.GetLocationInfo(location, key) == null,
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
                LocationComponent locationComponent = scene.GetComponent<LocationComponent>();
                MessageSender messageSender = scene.GetComponent<MessageSender>();

                ActorId selfActorId = scene.GetActorId();
                int zone = scene.Zone();
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, scene.Name, selfActorId, zone, 200);
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, "0_location_primary",
                    Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 301024, 1), zone, 100);
                locationComponent.RefreshPrimaryState();
                Actorlocation_TestHelper.AssertTrue(!locationComponent.IsPrimaryLocation, "backup-rejects/become-backup");
                Actorlocation_TestHelper.AssertEqual("0_location_primary", locationComponent.PrimaryLocationSceneName,
                    "backup-rejects/primary-name");

                using ObjectGetRequest request = ObjectGetRequest.Create();
                request.Type = LocationType;
                request.Key = IdGenerater.Instance.GenerateId();

                using ObjectGetResponse response = await messageSender.Call(selfActorId, request) as ObjectGetResponse;

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
