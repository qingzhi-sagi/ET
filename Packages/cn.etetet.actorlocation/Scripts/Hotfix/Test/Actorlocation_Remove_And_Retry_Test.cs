using System;
using ET.Server;

namespace ET.Test
{
    public class Actorlocation_LocationProxy_PrimaryUnavailableRetry_Test : ATestHandler
    {
        private const int LocationType = 910011;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_LocationProxy_PrimaryUnavailableRetry_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareProxyScene(scope.TestFiber);

                ServiceDiscoveryProxy serviceDiscoveryProxy = scene.GetComponent<ServiceDiscoveryProxy>();
                LocationProxyComponent locationProxy = scene.GetComponent<LocationProxyComponent>();
                TimerComponent timerComponent = scene.TimerComponent;

                EntityRef<ServiceDiscoveryProxy> serviceDiscoveryProxyRef = serviceDiscoveryProxy;
                EntityRef<LocationProxyComponent> locationProxyRef = locationProxy;
                EntityRef<TimerComponent> timerRef = timerComponent;

                locationProxy.locationRequestRetryTimes = 10;
                locationProxy.locationRequestRetryIntervalMs = 10;

                ActorId serviceActorId = scene.GetActorId();
                int zone = scene.Zone();
                long key = IdGenerater.Instance.GenerateId();
                ActorId actorId = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 401001, 1);

                async ETTask RegisterPrimaryLater()
                {
                    TimerComponent timer = timerRef;
                    if (timer == null)
                    {
                        return;
                    }

                    await timer.WaitAsync(30);
                    serviceDiscoveryProxy = serviceDiscoveryProxyRef;
                    if (serviceDiscoveryProxy == null)
                    {
                        return;
                    }

                    Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, "a_location_retry_later", serviceActorId, zone, 100);
                }

                RegisterPrimaryLater().Coroutine();

                await locationProxy.Add(LocationType, key, actorId);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("primary-unavailable-retry: location proxy disposed after add");
                }

                ActorId routedActorId = await locationProxy.Get(LocationType, key);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("primary-unavailable-retry: location proxy disposed after get");
                }

                Actorlocation_TestHelper.AssertActorEqual(actorId, routedActorId, "primary-unavailable-retry/routed-actor");
                Actorlocation_TestHelper.AssertEqual("a_location_retry_later", locationProxy.primaryLocationSceneName,
                    "primary-unavailable-retry/primary-selected");

                await locationProxy.Remove(LocationType, key);
                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Console($"Actorlocation_LocationProxy_PrimaryUnavailableRetry_Test failed: {e}");
                return 1;
            }
        }
    }

    public class Actorlocation_MessageLocationSender_SendMissingRoute_NoCache_Test : ATestHandler
    {
        private const int LocationType = 910012;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_MessageLocationSender_SendMissingRoute_NoCache_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareProxyScene(scope.TestFiber);

                ServiceDiscoveryProxy serviceDiscoveryProxy = scene.GetComponent<ServiceDiscoveryProxy>();
                LocationProxyComponent locationProxy = scene.GetComponent<LocationProxyComponent>();
                MessageLocationSenderComponent senderComponent = scene.GetComponent<MessageLocationSenderComponent>();
                TimerComponent timerComponent = scene.TimerComponent;

                EntityRef<LocationProxyComponent> locationProxyRef = locationProxy;
                EntityRef<MessageLocationSenderComponent> senderComponentRef = senderComponent;
                EntityRef<TimerComponent> timerRef = timerComponent;

                locationProxy.locationRequestRetryTimes = 3;
                locationProxy.locationRequestRetryIntervalMs = 5;

                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, "a_sender_missing_route",
                    scene.GetActorId(), scene.Zone(), 100);

                senderComponent = senderComponentRef;
                if (senderComponent == null)
                {
                    throw new Exception("send-missing-route: sender component disposed");
                }

                MessageLocationSenderOneType senderOneType = senderComponent.Get(LocationType);
                EntityRef<MessageLocationSenderOneType> senderOneTypeRef = senderOneType;
                long entityId = IdGenerater.Instance.GenerateId();

                M2C_TestResponse message = M2C_TestResponse.Create();
                try
                {
                    senderOneType.Send(entityId, message);

                    timerComponent = timerRef;
                    if (timerComponent == null)
                    {
                        throw new Exception("send-missing-route: timer disposed after send");
                    }

                    await timerComponent.WaitAsync(50);

                    senderOneType = senderOneTypeRef;
                    if (senderOneType == null)
                    {
                        throw new Exception("send-missing-route: sender one type disposed after send");
                    }

                    MessageLocationSender cachedSender = senderOneType.GetChild<MessageLocationSender>(entityId);
                    Actorlocation_TestHelper.AssertTrue(cachedSender == null, "send-missing-route/cache-removed");
                }
                finally
                {
                    message.Dispose();
                }

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Console($"Actorlocation_MessageLocationSender_SendMissingRoute_NoCache_Test failed: {e}");
                return 1;
            }
        }
    }

    public class Actorlocation_Remove_ActorMatch_And_Lock_Test : ATestHandler
    {
        private const int LocationType = 910013;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_Remove_ActorMatch_And_Lock_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareProxyScene(scope.TestFiber);

                ServiceDiscoveryProxy serviceDiscoveryProxy = scene.GetComponent<ServiceDiscoveryProxy>();
                LocationProxyComponent locationProxy = scene.GetComponent<LocationProxyComponent>();
                LocationOneType location = Actorlocation_TestHelper.GetLocationOneType(scene, LocationType);

                EntityRef<LocationProxyComponent> locationProxyRef = locationProxy;
                EntityRef<LocationOneType> locationRef = location;

                locationProxy.locationRequestRetryTimes = 3;
                locationProxy.locationRequestRetryIntervalMs = 1;

                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, "a_remove_actor_match",
                    scene.GetActorId(), scene.Zone(), 100);

                long key = IdGenerater.Instance.GenerateId();
                ActorId oldActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 401002, 1);
                ActorId newActor = scene.GetActorId();

                await locationProxy.Add(LocationType, key, oldActor);
                locationProxy = locationProxyRef;
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "remove-actor-match/add");
                if (locationProxy == null)
                {
                    throw new Exception("remove-actor-match: location proxy disposed after add");
                }

                long moveLockToken = await location.Lock(key, oldActor, 0);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "remove-actor-match/lock-before-move");
                await location.UnLock(key, oldActor, newActor, moveLockToken);

                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("remove-actor-match: location proxy disposed before stale remove");
                }

                await locationProxy.Remove(LocationType, key, oldActor);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("remove-actor-match: location proxy disposed after stale remove");
                }

                ActorId actorAfterStaleRemove = await locationProxy.Get(LocationType, key);
                locationProxy = locationProxyRef;
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "remove-actor-match/get-after-stale-remove");
                if (locationProxy == null)
                {
                    throw new Exception("remove-actor-match: location proxy disposed after stale remove get");
                }
                Actorlocation_TestHelper.AssertActorEqual(newActor, actorAfterStaleRemove, "remove-actor-match/stale-remove-skipped");

                long activeLockToken = await location.Lock(key, newActor, 0);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "remove-actor-match/lock-before-remove");

                await Actorlocation_TestHelper.ExpectRpcError(
                    async () =>
                    {
                        LocationProxyComponent currentLocationProxy = locationProxyRef;
                        if (currentLocationProxy == null)
                        {
                            throw new Exception("remove-actor-match: location proxy disposed during locked remove");
                        }

                        await currentLocationProxy.Remove(LocationType, key);
                    },
                    ErrorCode.ERR_LocationAlreadyLocked,
                    "remove-actor-match/locked-remove-rejected");

                locationProxy = locationProxyRef;
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "remove-actor-match/unlock-after-reject");
                if (locationProxy == null)
                {
                    throw new Exception("remove-actor-match: location proxy disposed after locked remove");
                }

                await location.UnLock(key, newActor, newActor, activeLockToken);

                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("remove-actor-match: location proxy disposed before final remove");
                }

                await locationProxy.Remove(LocationType, key);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("remove-actor-match: location proxy disposed after final remove");
                }

                ActorId actorAfterFinalRemove = await locationProxy.Get(LocationType, key);
                Actorlocation_TestHelper.AssertTrue(actorAfterFinalRemove == default, "remove-actor-match/final-remove-cleared");

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Console($"Actorlocation_Remove_ActorMatch_And_Lock_Test failed: {e}");
                return 1;
            }
        }
    }
}
