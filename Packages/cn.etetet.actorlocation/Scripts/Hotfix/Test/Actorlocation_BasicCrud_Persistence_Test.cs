using System;
using ET.Server;

namespace ET.Test
{
    public class Actorlocation_BasicCrud_Persistence_Test : ATestHandler
    {
        private const int LocationType = 910001;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_BasicCrud_Persistence_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareLocationScene(scope.TestFiber);
                LocationComponent location = Actorlocation_TestHelper.GetLocationComponent(scene);
                EntityRef<LocationComponent> locationRef = location;

                long key = IdGenerater.Instance.GenerateId();
                ActorId actorA = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 300001, 1);

                await location.Add(LocationType, key, actorA);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "basic/add");

                ActorId afterAdd = await location.Get(LocationType, key);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "basic/get-after-add");
                Actorlocation_TestHelper.AssertActorEqual(actorA, afterAdd, "basic/get-after-add");
                Actorlocation_TestHelper.AssertTrue(Actorlocation_TestHelper.GetLocationInfo(location, key) != null, "basic/cache-after-add");

                location.RemoveChild(key);
                ActorId reloadFromDb = await location.Get(LocationType, key);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "basic/get-after-cache-remove");
                Actorlocation_TestHelper.AssertActorEqual(actorA, reloadFromDb, "basic/reload-from-db");
                Actorlocation_TestHelper.AssertTrue(Actorlocation_TestHelper.GetLocationInfo(location, key) != null, "basic/cache-rehydrated");

                await location.Remove(LocationType, key);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "basic/remove");
                Actorlocation_TestHelper.AssertTrue(Actorlocation_TestHelper.GetLocationInfo(location, key) == null, "basic/cache-after-remove");

                ActorId afterRemove = await location.Get(LocationType, key);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "basic/get-after-remove");
                Actorlocation_TestHelper.AssertTrue(afterRemove == default, "basic/default-after-remove");

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Error($"Actorlocation_BasicCrud_Persistence_Test failed: {e}");
                return 1;
            }
        }
    }
}
