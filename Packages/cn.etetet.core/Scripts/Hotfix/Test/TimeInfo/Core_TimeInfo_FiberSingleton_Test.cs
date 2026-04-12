namespace ET.Test
{
    /// <summary>
    /// TimeInfo fiber singleton contract test
    /// </summary>
    public class Core_TimeInfo_FiberSingleton_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(
                context.Fiber, SceneType.TestEmpty, nameof(Core_TimeInfo_FiberSingleton_Test));

            bool createdGlobal = false;
            TimeInfo globalTimeInfo = scope.TestFiber.GetSingleton<TimeInfo>();
            if (globalTimeInfo == null)
            {
                globalTimeInfo = CreateTimeInfo(0);
                World.Instance.AddSingleton(globalTimeInfo);
                createdGlobal = true;
            }

            Scene scene = scope.TestFiber.Root;
            TimeInfo fiberTimeInfo = CreateTimeInfo(123456);
            scope.TestFiber.AddSingleton(fiberTimeInfo);

            try
            {
                TimeInfo sceneTimeInfo = scene.GetSingleton<TimeInfo>();
                if (!ReferenceEquals(sceneTimeInfo, fiberTimeInfo))
                {
                    Log.Console("time info scene singleton should use fiber override");
                    return 1;
                }

                TimeInfo directFiberTimeInfo = scope.TestFiber.GetSingleton<TimeInfo>();
                if (!ReferenceEquals(directFiberTimeInfo, fiberTimeInfo))
                {
                    Log.Console("time info fiber singleton should return fiber override");
                    return 2;
                }

                if (!scope.TestFiber.RemoveSingleton<TimeInfo>())
                {
                    Log.Console("time info fiber singleton remove should return true");
                    return 3;
                }

                TimeInfo fallbackTimeInfo = scene.GetSingleton<TimeInfo>();
                if (!ReferenceEquals(fallbackTimeInfo, globalTimeInfo))
                {
                    Log.Console("time info scene singleton should fall back to world singleton");
                    return 4;
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                scope.TestFiber.RemoveSingleton<TimeInfo>();
                fiberTimeInfo.Dispose();

                if (createdGlobal)
                {
                    World.Instance.RemoveSingleton<TimeInfo>();
                }
            }
        }

        private static TimeInfo CreateTimeInfo(long serverMinusClientTime)
        {
            TimeInfo timeInfo = new();
            timeInfo.Awake();
            timeInfo.ServerMinusClientTime = serverMinusClientTime;
            return timeInfo;
        }
    }
}
