using System;
using ET.Server;

namespace ET.Test
{
    public class Actorlocation_Get_LockRetry_Test : ATestHandler
    {
        private const int LocationType = 910008;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_Get_LockRetry_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareProxyScene(scope.TestFiber);
                _ = Actorlocation_TestHelper.EnsureAddressSingletonReady();

                LocationOneType location = Actorlocation_TestHelper.GetLocationOneType(scene, LocationType);
                EntityRef<LocationOneType> locationRef = location;

                LocationProxyComponent locationProxy = scene.GetComponent<LocationProxyComponent>();
                EntityRef<LocationProxyComponent> locationProxyRef = locationProxy;
                ServiceDiscoveryProxy serviceDiscoveryProxy = scene.GetComponent<ServiceDiscoveryProxy>();

                ActorId serviceActorId = scene.GetActorId();
                Actorlocation_TestHelper.AddLocalLocationService(serviceDiscoveryProxy, "a_get_lock_retry_mock", serviceActorId,
                    scene.Zone());

                locationProxy.locationRequestRetryTimes = 30;
                locationProxy.locationRequestRetryIntervalMs = 10;

                long key = IdGenerater.Instance.GenerateId();
                ActorId oldActor = Actorlocation_TestHelper.CreateActorId(301005, 1);
                ActorId newActor = Actorlocation_TestHelper.CreateActorId(301006, 1);
                TimerComponent timerComponent = scene.TimerComponent;
                EntityRef<TimerComponent> timerRef = timerComponent;

                await location.Add(key, oldActor);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "get-lock-retry/add");

                long lockToken = await location.Lock(key, oldActor, 0);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "get-lock-retry/lock");
                Actorlocation_TestHelper.AssertTrue(lockToken != 0, "get-lock-retry/lock-token");

                await Actorlocation_TestHelper.ExpectRpcError(
                    async () =>
                    {
                        LocationOneType current = Actorlocation_TestHelper.EnsureLocation(locationRef,
                            "get-lock-retry/direct-get");
                        await current.Get(key);
                    },
                    ErrorCode.ERR_LocationGetRetry,
                    "get-lock-retry/direct-get");

                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "get-lock-retry/clear-cache");
                location.RemoveChild(key);

                await Actorlocation_TestHelper.ExpectRpcError(
                    async () =>
                    {
                        LocationOneType current = Actorlocation_TestHelper.EnsureLocation(locationRef,
                            "get-lock-retry/direct-get-after-cache-clear");
                        await current.Get(key);
                    },
                    ErrorCode.ERR_LocationGetRetry,
                    "get-lock-retry/direct-get-after-cache-clear");

                async ETTask UnlockLater()
                {
                    TimerComponent timer = timerRef;
                    if (timer == null)
                    {
                        return;
                    }

                    await timer.WaitAsync(80);
                    LocationOneType current = locationRef;
                    if (current == null)
                    {
                        return;
                    }

                    await current.UnLock(key, oldActor, newActor, lockToken);
                }

                UnlockLater().Coroutine();

                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("get-lock-retry: location proxy disposed before get");
                }

                ActorId proxyGetActor = await locationProxy.Get(LocationType, key);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("get-lock-retry: location proxy disposed after get");
                }

                Actorlocation_TestHelper.AssertActorEqual(newActor, proxyGetActor, "get-lock-retry/proxy-get");

                await locationProxy.Remove(LocationType, key);
                locationProxy = locationProxyRef;
                if (locationProxy == null)
                {
                    throw new Exception("get-lock-retry: location proxy disposed after remove");
                }

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Console($"Actorlocation_Get_LockRetry_Test failed: {e}");
                return 1;
            }
        }
    }
}
