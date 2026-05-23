using System;
using ET.Server;

namespace ET.Test
{
    public class Actorlocation_LockWithoutAdd_UnlockSuccess_Test : ATestHandler
    {
        private const int LocationType = 910006;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_LockWithoutAdd_UnlockSuccess_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareLocationScene(scope.TestFiber);
                LocationComponent location = Actorlocation_TestHelper.GetLocationComponent(scene);
                EntityRef<LocationComponent> locationRef = location;

                long key = IdGenerater.Instance.GenerateId();
                ActorId oldActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300012, 1);
                ActorId newActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300013, 1);

                long lockToken = await location.Lock(LocationType, key, oldActor, 0);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "lock-without-add/lock");
                Actorlocation_TestHelper.AssertTrue(lockToken != 0, "lock-without-add/lock-token-empty");

                await location.UnLock(LocationType, key, oldActor, newActor, lockToken);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "lock-without-add/unlock");

                ActorId finalActor = await location.Get(LocationType, key);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "lock-without-add/get-final");
                Actorlocation_TestHelper.AssertActorEqual(newActor, finalActor, "lock-without-add/final-actor");

                await location.Remove(LocationType, key);
                Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "lock-without-add/cleanup-remove");

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Error($"Actorlocation_LockWithoutAdd_UnlockSuccess_Test failed: {e}");
                return 1;
            }
        }
    }
}
