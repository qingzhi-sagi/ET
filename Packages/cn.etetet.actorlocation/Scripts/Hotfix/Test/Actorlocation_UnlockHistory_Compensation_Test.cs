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
                LocationComponent location = Actorlocation_TestHelper.GetLocationComponent(scene);
                EntityRef<LocationComponent> locationRef = location;

                long key = IdGenerater.Instance.GenerateId();
                ActorId oldActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300010, 1);
                ActorId newActor = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300011, 1);

                await location.Add(LocationType, key, oldActor);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "compensate/add");

                long lockToken = await location.Lock(LocationType, key, oldActor, 0);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "compensate/lock");
                await location.UnLock(LocationType, key, oldActor, newActor, lockToken);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "compensate/unlock-first");

                try
                {
                    LocationComponent current = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "compensate/unlock-second");
                    await current.UnLock(LocationType, key, oldActor, newActor, lockToken);
                    throw new Exception(
                        $"compensate/unlock-second: expected RpcException({ErrorCode.ERR_LocationLockNotFound}), but no exception");
                }
                catch (RpcException e)
                {
                    Actorlocation_TestHelper.AssertRpcError(
                        e,
                        ErrorCode.ERR_LocationLockNotFound,
                        "compensate/unlock-second");
                }

                location.RemoveChild(key);
                ActorId persistedActor = await location.Get(LocationType, key);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "compensate/get-persisted");
                Actorlocation_TestHelper.AssertActorEqual(newActor, persistedActor, "compensate/persisted-actor");

                await location.Remove(LocationType, key);
                Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "compensate/cleanup-remove");

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
