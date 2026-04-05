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
                if (initTimerComponent == null)
                {
                    throw new Exception("timeout: timer component not found");
                }

                EntityRef<TimerComponent> timerRef = initTimerComponent;
                LocationOneType location = Actorlocation_TestHelper.GetLocationOneType(scene, LocationType);
                EntityRef<LocationOneType> locationRef = location;

                long key = IdGenerater.Instance.GenerateId();
                ActorId actorA = Actorlocation_TestHelper.CreateActorId(300007, 1);
                ActorId actorC = Actorlocation_TestHelper.CreateActorId(300009, 1);

                await location.Add(key, actorA);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "timeout/add");

                long firstToken = await location.Lock(key, actorA, 150);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "timeout/lock-first");
                Actorlocation_TestHelper.AssertTrue(firstToken != 0, "timeout/first-token-not-zero");
                LocationInfo firstInfo = location.GetChild<LocationInfo>(key);
                Actorlocation_TestHelper.AssertTrue(firstInfo != null && firstInfo.LockToken != 0, "timeout/lock-exists-immediately");

                TimerComponent waitTimerComponent = timerRef;
                if (waitTimerComponent == null)
                {
                    throw new Exception("timeout: timer component disposed before wait");
                }

                await waitTimerComponent.WaitAsync(260);

                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "timeout/get-after-expire-before-call");
                ActorId actorAfterExpire = await location.Get(key);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "timeout/get-after-expire");
                Actorlocation_TestHelper.AssertActorEqual(actorA, actorAfterExpire, "timeout/get-after-expire");

                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "timeout/after-expire-cleanup");
                LocationInfo unlockedInfo = location.GetChild<LocationInfo>(key);
                Actorlocation_TestHelper.AssertTrue(unlockedInfo == null || unlockedInfo.LockToken == 0, "timeout/lock-removed");
                Actorlocation_TestHelper.AssertTrue(unlockedInfo == null || unlockedInfo.LockExpireTime == 0, "timeout/expire-time-removed");

                long secondToken = await location.Lock(key, actorA, 0);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "timeout/lock-second");
                Actorlocation_TestHelper.AssertTrue(secondToken != 0, "timeout/second-token-not-zero");

                await location.UnLock(key, actorA, actorC, secondToken);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "timeout/unlock-second");

                ActorId finalActor = await location.Get(key);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "timeout/get-final");
                Actorlocation_TestHelper.AssertActorEqual(actorC, finalActor, "timeout/final-actor");

                await location.Remove(key);
                Actorlocation_TestHelper.EnsureLocation(locationRef, "timeout/cleanup-remove");

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
