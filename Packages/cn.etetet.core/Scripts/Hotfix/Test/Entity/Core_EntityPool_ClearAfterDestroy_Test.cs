namespace ET.Test
{
    [EntitySystemOf(typeof(EntityPoolClearTestEntity))]
    public static partial class EntityPoolClearTestEntitySystem
    {
        [EntitySystem]
        private static void Awake(this EntityPoolClearTestEntity self, EntityPoolClearRecorder recorder)
        {
            self.AwakeValue = 1;
            self.DestroyValue = 0;
            self.ClearValue = 10;
            self.Text = "created";
            self.Recorder = recorder;
        }

        [EntitySystem]
        private static void Destroy(this EntityPoolClearTestEntity self)
        {
            EntityPoolClearRecorder recorder = self.Recorder;
            if (recorder != null)
            {
                recorder.DestroySawClearValue = self.ClearValue;
            }

            self.DestroyValue = 2;
        }
    }
    
    public class Core_EntityPool_ClearAfterDestroy_Test: ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(
                context.Fiber, SceneType.TestEmpty, nameof(Core_EntityPool_ClearAfterDestroy_Test));

            Scene scene = scope.TestFiber.Root;
            EntityPoolClearRecorder recorder = scene.AddChild<EntityPoolClearRecorder>();
            EntityPoolClearTestEntity entity = scene.AddChild<EntityPoolClearTestEntity, EntityPoolClearRecorder>(recorder, true);

            if (!entity.IsFromPool)
            {
                Log.Console("Pool entity should be marked IsFromPool");
                return 1;
            }

            entity.Text = "dirty";
            entity.ClearValue = 10;
            entity.Dispose();

            if (recorder.DestroySawClearValue != 10)
            {
                Log.Console($"Destroy should run before Clear, actual ClearValue seen by Destroy: {recorder.DestroySawClearValue}");
                return 2;
            }

            if (entity.Text != null || entity.ClearValue != 0 || entity.DestroyValue != 0)
            {
                Log.Console($"Clear should reset disposed pooled entity fields, Text: {entity.Text}, ClearValue: {entity.ClearValue}, DestroyValue: {entity.DestroyValue}");
                return 3;
            }

            EntityPoolClearTestEntity reused = scene.AddChild<EntityPoolClearTestEntity, EntityPoolClearRecorder>(recorder, true);
            if (!reused.IsFromPool)
            {
                Log.Console("Reused entity should be marked IsFromPool");
                return 4;
            }

            if (reused.Text != "created" || reused.ClearValue != 10 || reused.AwakeValue != 1)
            {
                Log.Console($"Awake should initialize reused entity, Text: {reused.Text}, ClearValue: {reused.ClearValue}, AwakeValue: {reused.AwakeValue}");
                return 5;
            }

            EntityPoolClearTestEntity normal = scene.AddChild<EntityPoolClearTestEntity, EntityPoolClearRecorder>(recorder);
            if (normal.IsFromPool)
            {
                Log.Console("Normal AddChild should not mark entity IsFromPool");
                return 6;
            }

            EntityPoolClearTestComponent component = scene.AddComponent<EntityPoolClearTestComponent>(true);
            if (!component.IsFromPool)
            {
                Log.Console("AddComponent with isFromPool should mark component IsFromPool");
                return 7;
            }

            component.Value = 11;
            component.Text = "dirty";
            component.Dispose();
            if (component.Value != 0 || component.Text != null)
            {
                Log.Console($"AddComponent with isFromPool should clear component fields, Value: {component.Value}, Text: {component.Text}");
                return 8;
            }

            EntityPoolClearWithIdTestComponent componentWithId = scene.AddComponentWithId<EntityPoolClearWithIdTestComponent>(scene.Id, true);
            if (!componentWithId.IsFromPool)
            {
                Log.Console("AddComponentWithId with isFromPool should mark component IsFromPool");
                return 9;
            }

            componentWithId.Value = 15;
            componentWithId.Text = "dirty";
            componentWithId.Dispose();
            if (componentWithId.Value != 0 || componentWithId.Text != null)
            {
                Log.Console($"AddComponentWithId with isFromPool should clear component fields, Value: {componentWithId.Value}, Text: {componentWithId.Text}");
                return 10;
            }
            
            GeneratedIPoolClearWithBusinessClear businessClear = ObjectPool.Fetch<GeneratedIPoolClearWithBusinessClear>();
            businessClear.Value = 12;
            businessClear.Text = "dirty";
            businessClear.BusinessClearCalled = false;
            businessClear.Dispose();
            if (businessClear.Value != 0 || businessClear.Text != null || businessClear.BusinessClearCalled)
            {
                Log.Console($"IPool.Clear should be generated explicitly without calling business Clear, Value: {businessClear.Value}, Text: {businessClear.Text}, BusinessClearCalled: {businessClear.BusinessClearCalled}");
                return 11;
            }

            GeneratedIPoolListClearWithBusinessClear listClear = ObjectPool.Fetch<GeneratedIPoolListClearWithBusinessClear>();
            listClear.Add(1);
            listClear.Value = 13;
            listClear.BusinessClearCalled = false;
            listClear.Dispose();
            if (listClear.Count != 0 || listClear.Value != 0 || listClear.BusinessClearCalled)
            {
                Log.Console($"Generated IPool.Clear should clear base collection without calling business Clear, Count: {listClear.Count}, Value: {listClear.Value}, BusinessClearCalled: {listClear.BusinessClearCalled}");
                return 12;
            }

            GeneratedIPoolCollectionMemberClear collectionMemberClear = ObjectPool.Fetch<GeneratedIPoolCollectionMemberClear>();
            collectionMemberClear.Items.Add(1);
            collectionMemberClear.Names.Add(1, "one");
            collectionMemberClear.Value = 14;
            collectionMemberClear.Dispose();
            if (collectionMemberClear.Items == null || collectionMemberClear.Names == null ||
                collectionMemberClear.Items.Count != 0 || collectionMemberClear.Names.Count != 0 || collectionMemberClear.Value != 0)
            {
                Log.Console($"Generated IPool.Clear should clear collection members, Items: {collectionMemberClear.Items?.Count}, Names: {collectionMemberClear.Names?.Count}, Value: {collectionMemberClear.Value}");
                return 13;
            }

            DictionaryComponent<int, string> dictionaryComponent = DictionaryComponent<int, string>.Create();
            dictionaryComponent.Add(1, "one");
            dictionaryComponent.Dispose();
            if (dictionaryComponent.Count != 0)
            {
                Log.Console($"Generated IPool.Clear should clear DictionaryComponent base dictionary, Count: {dictionaryComponent.Count}");
                return 14;
            }

            reused.Dispose();
            normal.Dispose();
            recorder.Dispose();

            Log.Debug("Core_EntityPool_ClearAfterDestroy_Test all passed");
            return ErrorCode.ERR_Success;
        }
    }
}
