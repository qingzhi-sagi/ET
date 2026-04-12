using System;
using ET.Server;

namespace ET.Test
{
    public class Actorlocation_UnlockHistory_Compensation_Test : ATestHandler
    {
        private const int LocationType = 910005;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_UnlockHistory_Compensation_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareLocationScene(scope.TestFiber);
                LocationOneType location = Actorlocation_TestHelper.GetLocationOneType(scene, LocationType);
                EntityRef<LocationOneType> locationRef = location;

                long key = IdGenerater.Instance.GenerateId();
                ActorId oldActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300010, 1);
                ActorId newActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300011, 1);

                await location.Add(key, oldActor);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "compensate/add");

                long lockToken = await location.Lock(key, oldActor, 0);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "compensate/lock");
                await location.UnLock(key, oldActor, newActor, lockToken);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "compensate/unlock-first");

                await Actorlocation_TestHelper.ExpectRpcError(
                    async () =>
                    {
                        LocationOneType current = Actorlocation_TestHelper.EnsureLocation(locationRef, "compensate/unlock-second");
                        await current.UnLock(key, oldActor, newActor, lockToken);
                    },
                    ErrorCode.ERR_LocationLockNotFound,
                    "compensate/unlock-second");

                location.RemoveChild(key);
                ActorId persistedActor = await location.Get(key);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "compensate/get-persisted");
                Actorlocation_TestHelper.AssertActorEqual(newActor, persistedActor, "compensate/persisted-actor");

                await location.Remove(key);
                Actorlocation_TestHelper.EnsureLocation(locationRef, "compensate/cleanup-remove");

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Error($"Actorlocation_UnlockHistory_Compensation_Test failed: {e}");
                return 1;
            }
        }
    }
}
