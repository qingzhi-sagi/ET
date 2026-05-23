using System;
using ET.Server;

namespace ET.Test
{
    public class Actorlocation_LocationProxy_RetryAndCompensation_Test : ATestHandler
    {
        private const int LocationType = 910006;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_LocationProxy_RetryAndCompensation_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareProxyScene(scope.TestFiber);

                LocationProxyComponent locationProxy = scene.GetComponent<LocationProxyComponent>();
                ServiceDiscoveryProxy serviceDiscoveryProxy = scene.GetComponent<ServiceDiscoveryProxy>();
                LocationComponent location = Actorlocation_TestHelper.GetLocationComponent(scene);

                EntityRef<LocationProxyComponent> locationProxyRef = locationProxy;
                EntityRef<LocationComponent> locationRef = location;

                ActorId serviceActorId = scene.GetActorId();
                int zone = scene.Zone();
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, "z_location_mock", serviceActorId, zone, 200);
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, "a_location_mock", serviceActorId, zone, 100);

                locationProxy.locationRequestRetryTimes = 3;
                locationProxy.locationRequestRetryIntervalMs = 1;

                long key = IdGenerater.Instance.GenerateId();
                ActorId oldActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 301001, 1);
                ActorId newActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 301002, 1);

                await locationProxy.Add(LocationType, key, oldActor);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("proxy test: location proxy disposed");
                }

                Actorlocation_TestHelper.AssertEqual("a_location_mock", locationProxy.primaryLocationSceneName,
                    "proxy/primary-selected");

                ActorId afterAdd = await locationProxy.Get(LocationType, key);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("proxy test: location proxy disposed after get");
                }
                Actorlocation_TestHelper.AssertActorEqual(oldActor, afterAdd, "proxy/add-route-written");

                long lockToken = await locationProxy.LockWithToken(LocationType, key, oldActor, 0);
                locationProxy = locationProxyRef;
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "proxy/location-after-lock");
                if (locationProxy == null)
                {
                    throw new Exception("proxy test: location proxy disposed after lock");
                }

                Actorlocation_TestHelper.AssertTrue(lockToken != 0, "proxy/lock-token");
                await location.UnLock(LocationType, key, oldActor, newActor, lockToken);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "proxy/location-after-direct-unlock");
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("proxy test: location proxy disposed before compensate unlock");
                }

                await locationProxy.UnLockWithRetry(LocationType, key, oldActor, newActor, lockToken);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("proxy test: location proxy disposed after unlock");
                }

                ActorId afterCompensation = await locationProxy.Get(LocationType, key);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("proxy test: location proxy disposed after compensation get");
                }
                Actorlocation_TestHelper.AssertActorEqual(newActor, afterCompensation, "proxy/compensation-final-actor");

                await locationProxy.Remove(LocationType, key);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("proxy test: location proxy disposed after remove");
                }

                ActorId afterRemove = await locationProxy.Get(LocationType, key);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("proxy test: location proxy disposed after remove get");
                }
                Actorlocation_TestHelper.AssertTrue(afterRemove == default, "proxy/remove-route");

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Console($"Actorlocation_LocationProxy_RetryAndCompensation_Test failed: {e}");
                return 1;
            }
        }
    }

    public class Actorlocation_MessageLocationSender_CacheAndRetry_Test : ATestHandler
    {
        private const int LocationType = 910007;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_MessageLocationSender_CacheAndRetry_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareProxyScene(scope.TestFiber);

                ServiceDiscoveryProxy serviceDiscoveryProxy = scene.GetComponent<ServiceDiscoveryProxy>();
                LocationProxyComponent locationProxy = scene.GetComponent<LocationProxyComponent>();
                MessageLocationSenderComponent senderComponent = scene.GetComponent<MessageLocationSenderComponent>();
                LocationComponent location = Actorlocation_TestHelper.GetLocationComponent(scene);
                TimerComponent timerComponent = scene.TimerComponent;

                EntityRef<LocationProxyComponent> locationProxyRef = locationProxy;
                EntityRef<MessageLocationSenderComponent> senderComponentRef = senderComponent;
                EntityRef<LocationComponent> locationRef = location;
                EntityRef<TimerComponent> timerRef = timerComponent;

                locationProxy.locationRequestRetryTimes = 3;
                locationProxy.locationRequestRetryIntervalMs = 1;

                ActorId serviceActorId = scene.GetActorId();
                int zone = scene.Zone();
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, "a_sender_location_mock", serviceActorId,
                    zone);

                long entityId = IdGenerater.Instance.GenerateId();
                ActorId missingActorId = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, scope.TestFiber.Id, int.MaxValue);
                await location.Add(LocationType, entityId, missingActorId);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "sender/add-missing-route");

                async ETTask SwitchRouteToValid()
                {
                    TimerComponent timer = timerRef;
                    if (timer == null)
                    {
                        return;
                    }

                    await timer.WaitAsync(60);
                    LocationComponent current = locationRef;
                    if (current == null)
                    {
                        return;
                    }

                    EntityRef<LocationComponent> currentRef = current;
                    long lockToken = await current.Lock(LocationType, entityId, missingActorId, 0);
                    current = currentRef;
                    if (current == null)
                    {
                        return;
                    }

                    await current.UnLock(LocationType, entityId, missingActorId, serviceActorId, lockToken);
                }

                SwitchRouteToValid().Coroutine();

                senderComponent = senderComponentRef;
                if (senderComponent == null)
                {
                    throw new Exception("sender test: sender component disposed before get one type");
                }
                MessageLocationSenderOneType senderOneType = senderComponent.Get(LocationType);
                EntityRef<MessageLocationSenderOneType> senderOneTypeRef = senderOneType;

                using C2M_TestRequest firstRequest = C2M_TestRequest.Create();
                firstRequest.request = "first";

                using M2C_TestResponse firstResponse = await senderOneType.Call(entityId, firstRequest) as M2C_TestResponse;
                senderOneType = senderOneTypeRef;
                locationProxy = locationProxyRef;
                senderComponent = senderComponentRef;
                location = locationRef;
                if (senderOneType == null || locationProxy == null || senderComponent == null || location == null)
                {
                    throw new Exception("sender test: component disposed after first call");
                }

                Actorlocation_TestHelper.AssertTrue(firstResponse != null, "sender/first-response-null");
                Actorlocation_TestHelper.AssertEqual(ErrorCode.ERR_Success, firstResponse.Error, "sender/first-response-error");
                Actorlocation_TestHelper.AssertEqual("echo:first", firstResponse.response, "sender/first-response-body");
                
                long resetLockToken = await location.Lock(LocationType, entityId, serviceActorId, 0);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "sender/lock-route-before-reset");
                await location.UnLock(LocationType, entityId, serviceActorId, missingActorId, resetLockToken);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "sender/reset-route-to-missing");
                senderOneType = senderOneTypeRef;
                if (senderOneType == null)
                {
                    throw new Exception("sender test: sender disposed before second call");
                }

                using C2M_TestRequest secondRequest = C2M_TestRequest.Create();
                secondRequest.request = "second";
                using M2C_TestResponse secondResponse = await senderOneType.Call(entityId, secondRequest) as M2C_TestResponse;

                senderOneType = senderOneTypeRef;
                if (senderOneType == null)
                {
                    throw new Exception("sender test: sender disposed after second call");
                }

                Actorlocation_TestHelper.AssertTrue(secondResponse != null, "sender/second-response-null");
                Actorlocation_TestHelper.AssertEqual(ErrorCode.ERR_Success, secondResponse.Error, "sender/second-response-error");
                Actorlocation_TestHelper.AssertEqual("echo:second", secondResponse.response, "sender/second-response-body");

                senderOneType.Remove(entityId);
                senderOneType = senderOneTypeRef;
                if (senderOneType == null)
                {
                    throw new Exception("sender test: sender disposed before remove assert");
                }
                Actorlocation_TestHelper.AssertTrue(!senderOneType.Children.ContainsKey(entityId), "sender/remove-cache");

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Console($"Actorlocation_MessageLocationSender_CacheAndRetry_Test failed: {e}");
                return 1;
            }
        }
    }

    public class Actorlocation_MessageLocationSender_PrimarySwitch_ActorMove_Test : ATestHandler
    {
        private const int LocationType = 910009;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_MessageLocationSender_PrimarySwitch_ActorMove_Test));

            Fiber secondaryFiber = null;
            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareProxyScene(scope.TestFiber);

                ServiceDiscoveryProxy serviceDiscoveryProxy = scene.GetComponent<ServiceDiscoveryProxy>();
                LocationProxyComponent locationProxy = scene.GetComponent<LocationProxyComponent>();
                MessageLocationSenderComponent senderComponent = scene.GetComponent<MessageLocationSenderComponent>();
                LocationComponent location = Actorlocation_TestHelper.GetLocationComponent(scene);

                EntityRef<ServiceDiscoveryProxy> serviceDiscoveryProxyRef = serviceDiscoveryProxy;
                EntityRef<LocationProxyComponent> locationProxyRef = locationProxy;
                EntityRef<MessageLocationSenderComponent> senderComponentRef = senderComponent;
                EntityRef<LocationComponent> locationRef = location;

                locationProxy.locationRequestRetryTimes = 30;
                locationProxy.locationRequestRetryIntervalMs = 10;

                ActorId locationServiceActorId = scene.GetActorId();
                int zone = scene.Zone();
                ActorId newActorId = scene.GetActorId();
                const string primaryServiceName = "a_sender_switch_primary";
                const string backupServiceName = "z_sender_switch_backup";
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, backupServiceName, locationServiceActorId, zone, 200);
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, primaryServiceName, locationServiceActorId, zone, 100);

                secondaryFiber = await scope.TestFiber.CreateFiber(
                    IdGenerater.Instance.GenerateId(),
                    SceneType.TestEmpty,
                    nameof(Actorlocation_MessageLocationSender_PrimarySwitch_ActorMove_Test) + "_secondary");
                Scene secondaryScene = Actorlocation_TestHelper.PrepareProxyScene(secondaryFiber);
                long entityId = IdGenerater.Instance.GenerateId();
                ActorId oldActorId = secondaryScene.GetActorId();

                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "primary-switch/add-old-route-before");
                await location.Add(LocationType, entityId, oldActorId);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "primary-switch/add-old-route");

                senderComponent = senderComponentRef;
                if (senderComponent == null)
                {
                    throw new Exception("primary-switch: sender component disposed");
                }

                MessageLocationSenderOneType senderOneType = senderComponent.Get(LocationType);
                EntityRef<MessageLocationSenderOneType> senderOneTypeRef = senderOneType;

                using C2M_TestRequest firstRequest = C2M_TestRequest.Create();
                firstRequest.request = "switch-first";
                using M2C_TestResponse firstResponse = await senderOneType.Call(entityId, firstRequest) as M2C_TestResponse;
                senderOneType = senderOneTypeRef;
                if (senderOneType == null)
                {
                    throw new Exception("primary-switch: sender one type disposed after first call");
                }
                Actorlocation_TestHelper.AssertTrue(firstResponse != null, "primary-switch/first-response-null");
                Actorlocation_TestHelper.AssertEqual(ErrorCode.ERR_Success, firstResponse.Error, "primary-switch/first-response-error");
                Actorlocation_TestHelper.AssertEqual("echo:switch-first", firstResponse.response, "primary-switch/first-response-body");

                MessageLocationSender cachedSender = senderOneType.GetChild<MessageLocationSender>(entityId);
                Actorlocation_TestHelper.AssertTrue(cachedSender != null, "primary-switch/cached-sender-exists");
                Actorlocation_TestHelper.AssertActorEqual(oldActorId, cachedSender.ActorId, "primary-switch/cached-old-actor");

                serviceDiscoveryProxy = serviceDiscoveryProxyRef;
                if (serviceDiscoveryProxy == null)
                {
                    throw new Exception("primary-switch: service discovery proxy disposed before switch");
                }
                Actorlocation_TestHelper.RemoveLocalLocationService(serviceDiscoveryProxy, primaryServiceName, locationServiceActorId, zone);

                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "primary-switch/move-route");
                await location.Remove(LocationType, entityId);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "primary-switch/remove-old-route-done");
                await location.Add(LocationType, entityId, newActorId);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "primary-switch/move-route-done");

                await scope.TestFiber.RemoveFiber(secondaryFiber.Id);
                secondaryFiber = null;

                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("primary-switch: location proxy disposed before check switched primary");
                }
                ActorId routedAfterSwitch = await locationProxy.Get(LocationType, entityId);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("primary-switch: location proxy disposed after switched get");
                }

                Actorlocation_TestHelper.AssertActorEqual(newActorId, routedAfterSwitch, "primary-switch/get-after-switch");
                Actorlocation_TestHelper.AssertEqual(backupServiceName, locationProxy.primaryLocationSceneName, "primary-switch/backup-selected");

                senderOneType = senderOneTypeRef;
                if (senderOneType == null)
                {
                    throw new Exception("primary-switch: sender one type disposed before second call");
                }

                using C2M_TestRequest secondRequest = C2M_TestRequest.Create();
                secondRequest.request = "switch-second";
                using M2C_TestResponse secondResponse = await senderOneType.Call(entityId, secondRequest) as M2C_TestResponse;
                senderOneType = senderOneTypeRef;
                if (senderOneType == null)
                {
                    throw new Exception("primary-switch: sender one type disposed after second call");
                }

                Actorlocation_TestHelper.AssertTrue(secondResponse != null, "primary-switch/second-response-null");
                Actorlocation_TestHelper.AssertEqual(ErrorCode.ERR_Success, secondResponse.Error, "primary-switch/second-response-error");
                Actorlocation_TestHelper.AssertEqual("echo:switch-second", secondResponse.response, "primary-switch/second-response-body");

                cachedSender = senderOneType.GetChild<MessageLocationSender>(entityId);
                Actorlocation_TestHelper.AssertTrue(cachedSender != null, "primary-switch/cached-sender-updated-exists");
                Actorlocation_TestHelper.AssertActorEqual(newActorId, cachedSender.ActorId, "primary-switch/cached-new-actor");

                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("primary-switch: location proxy disposed before cleanup");
                }
                await locationProxy.Remove(LocationType, entityId);

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Console($"Actorlocation_MessageLocationSender_PrimarySwitch_ActorMove_Test failed: {e}");
                return 1;
            }
            finally
            {
                if (secondaryFiber != null)
                {
                    await scope.TestFiber.RemoveFiber(secondaryFiber.Id);
                }
            }
        }
    }

    public class Actorlocation_MessageLocationSender_ThreeFiber_LockSwitchMove_Test : ATestHandler
    {
        private const int LocationType = 910010;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_MessageLocationSender_ThreeFiber_LockSwitchMove_Test));

            Fiber fiberA = null;
            Fiber fiberC = null;
            try
            {
                Scene sceneB = Actorlocation_TestHelper.PrepareProxyScene(scope.TestFiber);

                ServiceDiscoveryProxy serviceDiscoveryProxyB = sceneB.GetComponent<ServiceDiscoveryProxy>();
                LocationProxyComponent locationProxyB = sceneB.GetComponent<LocationProxyComponent>();
                MessageLocationSenderComponent senderComponentB = sceneB.GetComponent<MessageLocationSenderComponent>();
                LocationComponent location = Actorlocation_TestHelper.GetLocationComponent(sceneB);

                EntityRef<ServiceDiscoveryProxy> serviceDiscoveryProxyBRef = serviceDiscoveryProxyB;
                EntityRef<LocationProxyComponent> locationProxyBRef = locationProxyB;
                EntityRef<MessageLocationSenderComponent> senderComponentBRef = senderComponentB;
                EntityRef<LocationComponent> locationRef = location;

                int zone = sceneB.Zone();
                ActorId locationServiceActorId = sceneB.GetActorId();

                fiberA = await scope.TestFiber.CreateFiber(
                    IdGenerater.Instance.GenerateId(),
                    SceneType.TestEmpty,
                    nameof(Actorlocation_MessageLocationSender_ThreeFiber_LockSwitchMove_Test) + "_A");
                Scene sceneA = Actorlocation_TestHelper.PrepareProxyScene(fiberA);
                ServiceDiscoveryProxy serviceDiscoveryProxyA = sceneA.GetComponent<ServiceDiscoveryProxy>();
                LocationProxyComponent locationProxyA = sceneA.GetComponent<LocationProxyComponent>();
                EntityRef<ServiceDiscoveryProxy> serviceDiscoveryProxyARef = serviceDiscoveryProxyA;
                EntityRef<LocationProxyComponent> locationProxyARef = locationProxyA;
                ActorId actorAId = sceneA.GetActorId();

                fiberC = await scope.TestFiber.CreateFiber(
                    IdGenerater.Instance.GenerateId(),
                    SceneType.TestEmpty,
                    nameof(Actorlocation_MessageLocationSender_ThreeFiber_LockSwitchMove_Test) + "_C");
                Scene sceneC = Actorlocation_TestHelper.PrepareProxyScene(fiberC);
                ActorId actorCId = sceneC.GetActorId();

                locationProxyB = locationProxyBRef;
                locationProxyA = locationProxyARef;
                if (locationProxyB == null || locationProxyA == null)
                {
                    throw new Exception("three-fiber: location proxy disposed before setup");
                }
                locationProxyB.locationRequestRetryTimes = 30;
                locationProxyB.locationRequestRetryIntervalMs = 10;
                locationProxyA.locationRequestRetryTimes = 30;
                locationProxyA.locationRequestRetryIntervalMs = 10;

                const string primaryServiceName = "a_sender_three_fiber_primary";
                const string backupServiceName = "z_sender_three_fiber_backup";
                serviceDiscoveryProxyB = serviceDiscoveryProxyBRef;
                serviceDiscoveryProxyA = serviceDiscoveryProxyARef;
                if (serviceDiscoveryProxyB == null || serviceDiscoveryProxyA == null)
                {
                    throw new Exception("three-fiber: service discovery disposed before setup");
                }
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxyB, backupServiceName, locationServiceActorId, zone, 200);
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxyB, primaryServiceName, locationServiceActorId, zone, 100);
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxyA, backupServiceName, locationServiceActorId, zone, 200);
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxyA, primaryServiceName, locationServiceActorId, zone, 100);

                long entityId = IdGenerater.Instance.GenerateId();
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "three-fiber/add-route-before");
                await location.Add(LocationType, entityId, actorAId);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "three-fiber/add-route-after");

                senderComponentB = senderComponentBRef;
                if (senderComponentB == null)
                {
                    throw new Exception("three-fiber: sender component B disposed");
                }

                MessageLocationSenderOneType senderOneType = senderComponentB.Get(LocationType);
                EntityRef<MessageLocationSenderOneType> senderOneTypeRef = senderOneType;

                using C2M_TestRequest firstRequest = C2M_TestRequest.Create();
                firstRequest.request = "three-fiber-first";
                using M2C_TestResponse firstResponse = await senderOneType.Call(entityId, firstRequest) as M2C_TestResponse;
                senderOneType = senderOneTypeRef;
                if (senderOneType == null)
                {
                    throw new Exception("three-fiber: sender one type disposed after first call");
                }

                Actorlocation_TestHelper.AssertTrue(firstResponse != null, "three-fiber/first-response-null");
                Actorlocation_TestHelper.AssertEqual(ErrorCode.ERR_Success, firstResponse.Error, "three-fiber/first-response-error");
                Actorlocation_TestHelper.AssertEqual("echo:three-fiber-first", firstResponse.response, "three-fiber/first-response-body");

                MessageLocationSender cachedSender = senderOneType.GetChild<MessageLocationSender>(entityId);
                Actorlocation_TestHelper.AssertTrue(cachedSender != null, "three-fiber/cached-first-exists");
                Actorlocation_TestHelper.AssertActorEqual(actorAId, cachedSender.ActorId, "three-fiber/cached-first-actor-a");

                locationProxyA = locationProxyARef;
                if (locationProxyA == null)
                {
                    throw new Exception("three-fiber: location proxy A disposed before lock");
                }

                long lockToken = await locationProxyA.LockWithToken(LocationType, entityId, actorAId, 0);
                locationProxyA = locationProxyARef;
                if (locationProxyA == null)
                {
                    throw new Exception("three-fiber: location proxy A disposed after lock");
                }
                Actorlocation_TestHelper.AssertTrue(lockToken != 0, "three-fiber/lock-token");

                serviceDiscoveryProxyB = serviceDiscoveryProxyBRef;
                if (serviceDiscoveryProxyB == null)
                {
                    throw new Exception("three-fiber: service discovery B disposed before switch");
                }
                Actorlocation_TestHelper.RemoveLocalLocationService(serviceDiscoveryProxyB, primaryServiceName, locationServiceActorId, zone);

                serviceDiscoveryProxyA = serviceDiscoveryProxyARef;
                if (serviceDiscoveryProxyA == null)
                {
                    throw new Exception("three-fiber: service discovery A disposed before switch");
                }
                Actorlocation_TestHelper.RemoveLocalLocationService(serviceDiscoveryProxyA, primaryServiceName, locationServiceActorId, zone);

                locationProxyA = locationProxyARef;
                if (locationProxyA == null)
                {
                    throw new Exception("three-fiber: location proxy A disposed before unlock");
                }

                await locationProxyA.UnLock(LocationType, entityId, actorAId, actorCId, lockToken);
                locationProxyA = locationProxyARef;
                if (locationProxyA == null)
                {
                    throw new Exception("three-fiber: location proxy A disposed after unlock");
                }

                await scope.TestFiber.RemoveFiber(fiberA.Id);
                fiberA = null;

                locationProxyB = locationProxyBRef;
                if (locationProxyB == null)
                {
                    throw new Exception("three-fiber: location proxy B disposed before route verify");
                }

                ActorId routedAfterMove = await locationProxyB.Get(LocationType, entityId);
                locationProxyB = locationProxyBRef;
                if (locationProxyB == null)
                {
                    throw new Exception("three-fiber: location proxy B disposed after route verify");
                }
                Actorlocation_TestHelper.AssertActorEqual(actorCId, routedAfterMove, "three-fiber/get-after-move");
                Actorlocation_TestHelper.AssertEqual(backupServiceName, locationProxyB.primaryLocationSceneName, "three-fiber/backup-selected");

                senderOneType = senderOneTypeRef;
                if (senderOneType == null)
                {
                    throw new Exception("three-fiber: sender one type disposed before second call");
                }

                using C2M_TestRequest secondRequest = C2M_TestRequest.Create();
                secondRequest.request = "three-fiber-second";
                using M2C_TestResponse secondResponse = await senderOneType.Call(entityId, secondRequest) as M2C_TestResponse;
                senderOneType = senderOneTypeRef;
                if (senderOneType == null)
                {
                    throw new Exception("three-fiber: sender one type disposed after second call");
                }

                Actorlocation_TestHelper.AssertTrue(secondResponse != null, "three-fiber/second-response-null");
                Actorlocation_TestHelper.AssertEqual(ErrorCode.ERR_Success, secondResponse.Error, "three-fiber/second-response-error");
                Actorlocation_TestHelper.AssertEqual("echo:three-fiber-second", secondResponse.response, "three-fiber/second-response-body");

                cachedSender = senderOneType.GetChild<MessageLocationSender>(entityId);
                Actorlocation_TestHelper.AssertTrue(cachedSender != null, "three-fiber/cached-second-exists");
                Actorlocation_TestHelper.AssertActorEqual(actorCId, cachedSender.ActorId, "three-fiber/cached-second-actor-c");

                locationProxyB = locationProxyBRef;
                if (locationProxyB == null)
                {
                    throw new Exception("three-fiber: location proxy B disposed before cleanup");
                }
                await locationProxyB.Remove(LocationType, entityId);

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Console($"Actorlocation_MessageLocationSender_ThreeFiber_LockSwitchMove_Test failed: {e}");
                return 1;
            }
            finally
            {
                if (fiberA != null)
                {
                    await scope.TestFiber.RemoveFiber(fiberA.Id);
                }

                if (fiberC != null)
                {
                    await scope.TestFiber.RemoveFiber(fiberC.Id);
                }
            }
        }
    }
}
