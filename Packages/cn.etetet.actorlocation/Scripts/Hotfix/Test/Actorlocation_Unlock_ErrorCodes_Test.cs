using System;
using ET.Server;

namespace ET.Test
{
    public class Actorlocation_Unlock_ErrorCodes_Test : ATestHandler
    {
        private const int LocationType = 910003;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_Unlock_ErrorCodes_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareLocationScene(scope.TestFiber);
                LocationOneType location = Actorlocation_TestHelper.GetLocationOneType(scene, LocationType);
                EntityRef<LocationOneType> locationRef = location;

                long key = IdGenerater.Instance.GenerateId();
                ActorId oldActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300004, 1);
                ActorId newActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300005, 1);
                ActorId otherActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300006, 1);

                await location.Add(key, oldActor);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "errors/add");

                long fakeToken = 999001;

                await Actorlocation_TestHelper.ExpectRpcError(
                    async () =>
                    {
                        LocationOneType current = Actorlocation_TestHelper.EnsureLocation(locationRef, "errors/lock-owner-mismatch-before-lock");
                        await current.Lock(key, otherActor, 0);
                    },
                    ErrorCode.ERR_LocationLockOwnerMismatch,
                    "errors/lock-owner-mismatch-before-lock");

                await Actorlocation_TestHelper.ExpectRpcError(
                    async () =>
                    {
                        LocationOneType current = Actorlocation_TestHelper.EnsureLocation(locationRef, "errors/unlock-without-lock");
                        await current.UnLock(key, oldActor, newActor, fakeToken);
                    },
                    ErrorCode.ERR_LocationLockNotFound,
                    "errors/unlock-without-lock");

                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "errors/lock-for-next-cases");
                long lockToken = await location.Lock(key, oldActor, 0);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "errors/lock-created");

                await Actorlocation_TestHelper.ExpectRpcError(
                    async () =>
                    {
                        LocationOneType current = Actorlocation_TestHelper.EnsureLocation(locationRef, "errors/lock-already-locked");
                        await current.Lock(key, otherActor, 0);
                    },
                    ErrorCode.ERR_LocationAlreadyLocked,
                    "errors/lock-already-locked");

                await Actorlocation_TestHelper.ExpectRpcError(
                    async () =>
                    {
                        LocationOneType current = Actorlocation_TestHelper.EnsureLocation(locationRef, "errors/unlock-owner-mismatch");
                        await current.UnLock(key, otherActor, newActor, lockToken);
                    },
                    ErrorCode.ERR_LocationLockOwnerMismatch,
                    "errors/unlock-owner-mismatch");

                long wrongToken = lockToken == long.MaxValue ? lockToken - 1 : lockToken + 1;
                await Actorlocation_TestHelper.ExpectRpcError(
                    async () =>
                    {
                        LocationOneType current = Actorlocation_TestHelper.EnsureLocation(locationRef, "errors/unlock-token-mismatch");
                        await current.UnLock(key, oldActor, newActor, wrongToken);
                    },
                    ErrorCode.ERR_LocationLockTokenMismatch,
                    "errors/unlock-token-mismatch");

                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "errors/unlock-final");
                await location.UnLock(key, oldActor, newActor, lockToken);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "errors/unlock-final-done");

                ActorId finalActor = await location.Get(key);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "errors/get-final");
                Actorlocation_TestHelper.AssertActorEqual(newActor, finalActor, "errors/final-actor");

                await location.Remove(key);
                Actorlocation_TestHelper.EnsureLocation(locationRef, "errors/cleanup-remove");

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Error($"Actorlocation_Unlock_ErrorCodes_Test failed: {e}");
                return 1;
            }
        }
    }
}
