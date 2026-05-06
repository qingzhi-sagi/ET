namespace ET.Test
{
    [TestExecution(TestExecutionMode.Exclusive)]
    public class Core_Fiber_SingletonContract_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Core_Fiber_SingletonContract_Test));
            Fiber testFiber = scope.TestFiber;

            int ret = VerifyFiberLocalSingleton(testFiber);
            if (ret != 0)
            {
                return ret;
            }

            await ETTask.CompletedTask;
            return ErrorCode.ERR_Success;
        }

        private static int VerifyFiberLocalSingleton(Fiber fiber)
        {
            World.Instance.RemoveSingleton<FiberSingletonContract>();
            FiberSingletonContract global = World.Instance.AddSingleton<FiberSingletonContract, int, string>(1001, "global");
            FiberSingletonContract fiberLocal = new();
            fiberLocal.Awake(1001, "fiber-local");

            try
            {
                fiber.AddSingleton(fiberLocal);
                if (!ReferenceEquals(fiber.GetSingleton<FiberSingletonContract>(), fiberLocal))
                {
                    Log.Console("fiber should return local singleton before global singleton");
                    return 1;
                }

                if (fiber.GetSingleton<FiberSingletonContract>().Name != "fiber-local")
                {
                    Log.Console("fiber should read local singleton data");
                    return 2;
                }

                if (!fiber.RemoveSingleton<FiberSingletonContract>())
                {
                    Log.Console("fiber should remove local singleton");
                    return 3;
                }

                if (!ReferenceEquals(fiber.GetSingleton<FiberSingletonContract>(), global))
                {
                    Log.Console("fiber should fall back to global singleton after local singleton removed");
                    return 4;
                }

                if (fiber.GetSingleton<FiberSingletonContract>().Name != "global")
                {
                    Log.Console("fiber should read global singleton data after local singleton removed");
                    return 5;
                }

                if (fiber.RemoveSingleton<FiberSingletonContract>())
                {
                    Log.Console("fiber should report no local singleton after it was removed");
                    return 6;
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                fiber.RemoveSingleton<FiberSingletonContract>();
                World.Instance.RemoveSingleton<FiberSingletonContract>();
            }
        }
    }
}
