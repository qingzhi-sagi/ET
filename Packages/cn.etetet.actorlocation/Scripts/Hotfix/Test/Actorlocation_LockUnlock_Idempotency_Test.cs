using System;
using ET.Server;

namespace ET.Test
{
    public class Actorlocation_LockUnlock_Idempotency_Test : ATestHandler
    {
        private const int LocationType = 910002;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_LockUnlock_Idempotency_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareLocationScene(scope.TestFiber);
                LocationComponent location = Actorlocation_TestHelper.GetLocationComponent(scene);
                EntityRef<LocationComponent> locationRef = location;

                long key = IdGenerater.Instance.GenerateId();
                ActorId oldActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300002, 1);
                ActorId newActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300003, 1);

                await location.Add(LocationType, key, oldActor);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "idempotency/add");

                long firstToken = await location.Lock(LocationType, key, oldActor, 0);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "idempotency/lock-first");
                long secondToken = await location.Lock(LocationType, key, oldActor, 0);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "idempotency/lock-second");

                Actorlocation_TestHelper.AssertEqual(firstToken, secondToken, "idempotency/lock-same-operation");

                await location.UnLock(LocationType, key, oldActor, newActor, firstToken);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "idempotency/unlock-first");

                try
                {
                    LocationComponent current = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "idempotency/unlock-second");
                    await current.UnLock(LocationType, key, oldActor, newActor, firstToken);
                    throw new Exception(
                        $"idempotency/unlock-second: expected RpcException({ErrorCode.ERR_LocationLockNotFound}), but no exception");
                }
                catch (RpcException e)
                {
                    Actorlocation_TestHelper.AssertRpcError(
                        e,
                        ErrorCode.ERR_LocationLockNotFound,
                        "idempotency/unlock-second");
                }

                ActorId finalActor = await location.Get(LocationType, key);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "idempotency/get-final");
                Actorlocation_TestHelper.AssertActorEqual(newActor, finalActor, "idempotency/final-actor");

                await location.Remove(LocationType, key);
                Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "idempotency/cleanup-remove");

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Error($"Actorlocation_LockUnlock_Idempotency_Test failed: {e}");
                return 1;
            }
        }
    }
}
