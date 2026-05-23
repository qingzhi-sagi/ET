using System;
using ET.Server;

namespace ET.Test
{
    public class Actorlocation_LockTimeout_AutoUnlock_Test : ATestHandler
    {
        private const int LocationType = 910004;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_LockTimeout_AutoUnlock_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareLocationScene(scope.TestFiber);
                TimerComponent initTimerComponent = scene.TimerComponent;
                EntityRef<TimerComponent> timerRef = initTimerComponent;
                LocationComponent location = Actorlocation_TestHelper.GetLocationComponent(scene);
                EntityRef<LocationComponent> locationRef = location;

                long key = IdGenerater.Instance.GenerateId();
                ActorId actorA = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300007, 1);
                ActorId actorC = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300009, 1);

                await location.Add(LocationType, key, actorA);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "timeout/add");

                long firstToken = await location.Lock(LocationType, key, actorA, 150);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "timeout/lock-first");
                Actorlocation_TestHelper.AssertTrue(firstToken != 0, "timeout/first-token-not-zero");
                LocationTypeState firstState = Actorlocation_TestHelper.GetLocationTypeState(location, LocationType, key,
                    "timeout/lock-exists-immediately");
                Actorlocation_TestHelper.AssertTrue(firstState.LockToken != 0, "timeout/lock-exists-immediately");

                TimerComponent waitTimerComponent = timerRef;
                if (waitTimerComponent == null)
                {
                    throw new Exception("timeout: timer component disposed before wait");
                }

                await waitTimerComponent.WaitAsync(260);

                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "timeout/get-after-expire-before-call");
                ActorId actorAfterExpire = await location.Get(LocationType, key);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "timeout/get-after-expire");
                Actorlocation_TestHelper.AssertActorEqual(actorA, actorAfterExpire, "timeout/get-after-expire");

                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "timeout/after-expire-cleanup");
                LocationTypeState unlockedState = Actorlocation_TestHelper.GetLocationTypeState(location, LocationType, key,
                    "timeout/after-expire-cleanup");
                Actorlocation_TestHelper.AssertTrue(unlockedState.LockToken == 0, "timeout/lock-removed");
                Actorlocation_TestHelper.AssertTrue(unlockedState.LockExpireTime == 0, "timeout/expire-time-removed");

                long secondToken = await location.Lock(LocationType, key, actorA, 0);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "timeout/lock-second");
                Actorlocation_TestHelper.AssertTrue(secondToken != 0, "timeout/second-token-not-zero");

                await location.UnLock(LocationType, key, actorA, actorC, secondToken);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "timeout/unlock-second");

                ActorId finalActor = await location.Get(LocationType, key);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "timeout/get-final");
                Actorlocation_TestHelper.AssertActorEqual(actorC, finalActor, "timeout/final-actor");

                await location.Remove(LocationType, key);
                Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "timeout/cleanup-remove");

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Error($"Actorlocation_LockTimeout_AutoUnlock_Test failed: {e}");
                return 1;
            }
        }
    }
}
