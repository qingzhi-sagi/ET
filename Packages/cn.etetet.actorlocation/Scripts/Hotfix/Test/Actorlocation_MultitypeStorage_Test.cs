using System;
using ET.Server;

namespace ET.Test
{
    public class Actorlocation_MultitypeStorage_Test: ATestHandler
    {
        private const int LocationTypeA = 920001;
        private const int LocationTypeB = 920002;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_MultitypeStorage_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareLocationScene(scope.TestFiber);
                LocationComponent locationComponent = scene.GetComponent<LocationComponent>();
                EntityRef<LocationComponent> locationComponentRef = locationComponent;

                long key = IdGenerater.Instance.GenerateId();
                ActorId actorA = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 920001, 1);
                ActorId actorB = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 920002, 1);

                await locationComponent.Add(LocationTypeA, key, actorA);
                locationComponent = locationComponentRef;
                if (locationComponent == null)
                {
                    throw new Exception("multitype/add-a: manager disposed");
                }

                await locationComponent.Add(LocationTypeB, key, actorB);
                locationComponent = locationComponentRef;
                if (locationComponent == null)
                {
                    throw new Exception("multitype/add-b: manager disposed");
                }

                locationComponent.ClearAllCache();

                ActorId loadedA = await locationComponent.Get(LocationTypeA, key);
                locationComponent = locationComponentRef;
                if (locationComponent == null)
                {
                    throw new Exception("multitype/get-a: manager disposed");
                }

                ActorId loadedB = await locationComponent.Get(LocationTypeB, key);
                locationComponent = locationComponentRef;
                if (locationComponent == null)
                {
                    throw new Exception("multitype/get-b: manager disposed");
                }

                Actorlocation_TestHelper.AssertActorEqual(actorA, loadedA, "multitype/type-a");
                Actorlocation_TestHelper.AssertActorEqual(actorB, loadedB, "multitype/type-b");

                await locationComponent.Remove(LocationTypeA, key);
                locationComponent = locationComponentRef;
                if (locationComponent == null)
                {
                    throw new Exception("multitype/remove-a: manager disposed");
                }

                locationComponent.ClearAllCache();
                ActorId afterRemoveA = await locationComponent.Get(LocationTypeA, key);
                locationComponent = locationComponentRef;
                if (locationComponent == null)
                {
                    throw new Exception("multitype/get-after-remove-a: manager disposed");
                }

                ActorId afterRemoveB = await locationComponent.Get(LocationTypeB, key);
                Actorlocation_TestHelper.AssertActorEqual(default, afterRemoveA, "multitype/remove-a-only");
                Actorlocation_TestHelper.AssertActorEqual(actorB, afterRemoveB, "multitype/type-b-still-exists");

                locationComponent = locationComponentRef;
                if (locationComponent != null)
                {
                    await locationComponent.Remove(LocationTypeB, key);
                }

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Console($"Actorlocation_MultitypeStorage_Test failed: {e}");
                return 1;
            }
        }
    }
}
