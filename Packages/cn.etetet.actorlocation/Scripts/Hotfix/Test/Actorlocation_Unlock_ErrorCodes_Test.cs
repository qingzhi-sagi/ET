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
                LocationComponent location = Actorlocation_TestHelper.GetLocationComponent(scene);
                EntityRef<LocationComponent> locationRef = location;

                long key = IdGenerater.Instance.GenerateId();
                ActorId oldActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300004, 1);
                ActorId newActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300005, 1);
                ActorId otherActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300006, 1);

                await location.Add(LocationType, key, oldActor);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/add");

                long fakeToken = 999001;

                try
                {
                    LocationComponent current = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/lock-owner-mismatch-before-lock");
                    await current.Lock(LocationType, key, otherActor, 0);
                    throw new Exception(
                        $"errors/lock-owner-mismatch-before-lock: expected RpcException({ErrorCode.ERR_LocationLockOwnerMismatch}), but no exception");
                }
                catch (RpcException e)
                {
                    Actorlocation_TestHelper.AssertRpcError(
                        e,
                        ErrorCode.ERR_LocationLockOwnerMismatch,
                        "errors/lock-owner-mismatch-before-lock");
                }

                try
                {
                    LocationComponent current = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/unlock-without-lock");
                    await current.UnLock(LocationType, key, oldActor, newActor, fakeToken);
                    throw new Exception(
                        $"errors/unlock-without-lock: expected RpcException({ErrorCode.ERR_LocationLockNotFound}), but no exception");
                }
                catch (RpcException e)
                {
                    Actorlocation_TestHelper.AssertRpcError(
                        e,
                        ErrorCode.ERR_LocationLockNotFound,
                        "errors/unlock-without-lock");
                }

                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/lock-for-next-cases");
                long lockToken = await location.Lock(LocationType, key, oldActor, 0);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/lock-created");

                try
                {
                    LocationComponent current = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/lock-already-locked");
                    await current.Lock(LocationType, key, otherActor, 0);
                    throw new Exception(
                        $"errors/lock-already-locked: expected RpcException({ErrorCode.ERR_LocationAlreadyLocked}), but no exception");
                }
                catch (RpcException e)
                {
                    Actorlocation_TestHelper.AssertRpcError(
                        e,
                        ErrorCode.ERR_LocationAlreadyLocked,
                        "errors/lock-already-locked");
                }

                try
                {
                    LocationComponent current = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/unlock-owner-mismatch");
                    await current.UnLock(LocationType, key, otherActor, newActor, lockToken);
                    throw new Exception(
                        $"errors/unlock-owner-mismatch: expected RpcException({ErrorCode.ERR_LocationLockOwnerMismatch}), but no exception");
                }
                catch (RpcException e)
                {
                    Actorlocation_TestHelper.AssertRpcError(
                        e,
                        ErrorCode.ERR_LocationLockOwnerMismatch,
                        "errors/unlock-owner-mismatch");
                }

                try
                {
                    LocationComponent current = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/unlock-zero-token");
                    await current.UnLock(LocationType, key, oldActor, newActor, 0);
                    throw new Exception(
                        $"errors/unlock-zero-token: expected RpcException({ErrorCode.ERR_LocationLockTokenMismatch}), but no exception");
                }
                catch (RpcException e)
                {
                    Actorlocation_TestHelper.AssertRpcError(
                        e,
                        ErrorCode.ERR_LocationLockTokenMismatch,
                        "errors/unlock-zero-token");
                }

                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/unlock-zero-token-state");
                LocationTypeState lockedState = Actorlocation_TestHelper.GetLocationTypeState(location, LocationType, key,
                    "errors/unlock-zero-token-state");
                Actorlocation_TestHelper.AssertActorEqual(oldActor, lockedState.ActorId, "errors/unlock-zero-token-state/actor");
                Actorlocation_TestHelper.AssertEqual(lockToken, lockedState.LockToken, "errors/unlock-zero-token-state/token");

                long wrongToken = lockToken == long.MaxValue ? lockToken - 1 : lockToken + 1;
                try
                {
                    LocationComponent current = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/unlock-token-mismatch");
                    await current.UnLock(LocationType, key, oldActor, newActor, wrongToken);
                    throw new Exception(
                        $"errors/unlock-token-mismatch: expected RpcException({ErrorCode.ERR_LocationLockTokenMismatch}), but no exception");
                }
                catch (RpcException e)
                {
                    Actorlocation_TestHelper.AssertRpcError(
                        e,
                        ErrorCode.ERR_LocationLockTokenMismatch,
                        "errors/unlock-token-mismatch");
                }

                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/unlock-final");
                await location.UnLock(LocationType, key, oldActor, newActor, lockToken);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/unlock-final-done");

                ActorId finalActor = await location.Get(LocationType, key);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/get-final");
                Actorlocation_TestHelper.AssertActorEqual(newActor, finalActor, "errors/final-actor");

                await location.Remove(LocationType, key);
                Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "errors/cleanup-remove");

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
