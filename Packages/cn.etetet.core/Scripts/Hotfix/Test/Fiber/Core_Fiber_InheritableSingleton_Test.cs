namespace ET.Test
{
    public class Core_Fiber_InheritableSingleton_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Core_Fiber_InheritableSingleton_Test));
            Fiber testFiber = scope.TestFiber;

            int ret = await VerifyInheritableSingleton(testFiber);
            if (ret != 0)
            {
                return ret;
            }

            ret = await VerifyForceParentSchedulerSingleton(testFiber);
            if (ret != 0)
            {
                return ret;
            }

            return ErrorCode.ERR_Success;
        }

        private static async ETTask<int> VerifyInheritableSingleton(Fiber fiber)
        {
            InheritableFiberSingletonContract singleton = new();
            singleton.Awake(8101);
            fiber.AddSingleton(singleton);

            Fiber child = await fiber.CreateFiber(IdGenerater.Instance.GenerateId(), SceneType.TestEmpty, "InheritableChild");
            try
            {
                InheritableFiberSingletonContract inherited = child.GetSingleton<InheritableFiberSingletonContract>();
                if (!ReferenceEquals(inherited, singleton))
                {
                    Log.Console("parent scheduler child fiber should inherit inheritable singleton");
                    return 1;
                }

                inherited.Value = 8102;
                if (singleton.Value != 8102)
                {
                    Log.Console("inherited singleton should be shared instance");
                    return 2;
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                await fiber.RemoveFiber(child.Id);
                fiber.RemoveSingleton<InheritableFiberSingletonContract>();
            }
        }

        private static async ETTask<int> VerifyForceParentSchedulerSingleton(Fiber fiber)
        {
            InheritableFiberSingletonContract singleton = new();
            singleton.Awake(8201);
            fiber.AddSingleton(singleton);

            long childId = await fiber.CreateFiber(SchedulerType.ThreadPool, IdGenerater.Instance.GenerateId(), SceneType.TestEmpty,
                "ForcedParentChild");
            Fiber child = fiber.GetFiber(childId);
            try
            {
                if (child == null || child.SchedulerType != SchedulerType.Parent)
                {
                    Log.Console("force parent scheduler singleton should create child as parent scheduler");
                    return 3;
                }

                if (!ReferenceEquals(child.GetSingleton<InheritableFiberSingletonContract>(), singleton))
                {
                    Log.Console("forced parent child should inherit inheritable singleton");
                    return 4;
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                await fiber.RemoveFiber(childId);
                fiber.RemoveSingleton<InheritableFiberSingletonContract>();
            }
        }
    }
}
