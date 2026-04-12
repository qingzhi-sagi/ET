using System;
using System.Collections.Generic;
using ET.Server;

namespace ET.Test
{
    public class Test_Fiber_ConfigCategory_SingletonContract_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope =
                    await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty, nameof(Test_Fiber_ConfigCategory_SingletonContract_Test));
            Fiber testFiber = scope.TestFiber;
            List<Action> cleanup = new();

            try
            {
                int ret = VerifyUnitConfigCategory(testFiber, cleanup);
                if (ret != 0)
                {
                    return ret;
                }

                ret = VerifyBuffConfigCategory(testFiber, cleanup);
                if (ret != 0)
                {
                    return ret;
                }

                ret = VerifySpellConfigCategory(testFiber, cleanup);
                if (ret != 0)
                {
                    return ret;
                }

                ret = VerifyStartMachineConfigCategory(testFiber, cleanup);
                if (ret != 0)
                {
                    return ret;
                }

                ret = VerifyStartProcessConfigCategory(testFiber, cleanup);
                if (ret != 0)
                {
                    return ret;
                }

                ret = VerifyStartSceneConfigCategory(testFiber, cleanup);
                if (ret != 0)
                {
                    return ret;
                }

                ret = VerifyStartZoneConfigCategory(testFiber, cleanup);
                if (ret != 0)
                {
                    return ret;
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                for (int i = cleanup.Count - 1; i >= 0; --i)
                {
                    cleanup[i]();
                }
            }
        }

        private static int VerifyUnitConfigCategory(Fiber fiber, List<Action> cleanup)
        {
            UnitConfigCategory global = EnsureWorldSingleton(
                fiber,
                cleanup,
                () => FiberConfigSingletonMockFactory.CreateUnitConfigCategory(1001, "global-unit"));
            UnitConfigCategory mock = FiberConfigSingletonMockFactory.CreateUnitConfigCategory(1001, "fiber-unit");

            fiber.AddSingleton(mock);
            if (!ReferenceEquals(fiber.GetSingleton<UnitConfigCategory>(), mock))
            {
                return Fail(1, "fiber should return unit config mock singleton");
            }

            if (fiber.GetSingleton<UnitConfigCategory>().Get(1001).Name != "fiber-unit")
            {
                return Fail(2, "fiber should read mocked unit config data");
            }

            if (!fiber.RemoveSingleton<UnitConfigCategory>())
            {
                return Fail(3, "fiber should remove unit config mock singleton");
            }

            if (!ReferenceEquals(fiber.GetSingleton<UnitConfigCategory>(), global))
            {
                return Fail(4, "fiber should fall back to global unit config singleton");
            }

            return ErrorCode.ERR_Success;
        }

        private static int VerifyBuffConfigCategory(Fiber fiber, List<Action> cleanup)
        {
            BuffConfigCategory global = EnsureWorldSingleton(
                fiber,
                cleanup,
                () => FiberConfigSingletonMockFactory.CreateBuffConfigCategory(2001, "global-buff"));
            BuffConfigCategory mock = FiberConfigSingletonMockFactory.CreateBuffConfigCategory(2001, "fiber-buff");

            fiber.AddSingleton(mock);
            if (!ReferenceEquals(fiber.GetSingleton<BuffConfigCategory>(), mock))
            {
                return Fail(5, "fiber should return buff config mock singleton");
            }

            if (fiber.GetSingleton<BuffConfigCategory>().Get(2001)?.Desc != "fiber-buff")
            {
                return Fail(6, "fiber should read mocked buff config data");
            }

            if (!fiber.RemoveSingleton<BuffConfigCategory>())
            {
                return Fail(7, "fiber should remove buff config mock singleton");
            }

            if (!ReferenceEquals(fiber.GetSingleton<BuffConfigCategory>(), global))
            {
                return Fail(8, "fiber should fall back to global buff config singleton");
            }

            return ErrorCode.ERR_Success;
        }

        private static int VerifySpellConfigCategory(Fiber fiber, List<Action> cleanup)
        {
            SpellConfigCategory global = EnsureWorldSingleton(
                fiber,
                cleanup,
                () => FiberConfigSingletonMockFactory.CreateSpellConfigCategory(3001, 2001, "global-spell"));
            SpellConfigCategory mock = FiberConfigSingletonMockFactory.CreateSpellConfigCategory(3001, 2001, "fiber-spell");

            fiber.AddSingleton(mock);
            if (!ReferenceEquals(fiber.GetSingleton<SpellConfigCategory>(), mock))
            {
                return Fail(9, "fiber should return spell config mock singleton");
            }

            if (fiber.GetSingleton<SpellConfigCategory>().Get(3001)?.Desc != "fiber-spell")
            {
                return Fail(10, "fiber should read mocked spell config data");
            }

            if (!fiber.RemoveSingleton<SpellConfigCategory>())
            {
                return Fail(11, "fiber should remove spell config mock singleton");
            }

            if (!ReferenceEquals(fiber.GetSingleton<SpellConfigCategory>(), global))
            {
                return Fail(12, "fiber should fall back to global spell config singleton");
            }

            return ErrorCode.ERR_Success;
        }

        private static int VerifyStartMachineConfigCategory(Fiber fiber, List<Action> cleanup)
        {
            StartMachineConfigCategory global = EnsureWorldSingleton(
                fiber,
                cleanup,
                () => FiberConfigSingletonMockFactory.CreateStartMachineConfigCategory(4001, "10.0.0.1", "20.0.0.1"));
            StartMachineConfigCategory mock = FiberConfigSingletonMockFactory.CreateStartMachineConfigCategory(4001, "10.1.0.1", "20.1.0.1");

            fiber.AddSingleton(mock);
            if (!ReferenceEquals(fiber.GetSingleton<StartMachineConfigCategory>(), mock))
            {
                return Fail(13, "fiber should return start machine config mock singleton");
            }

            if (fiber.GetSingleton<StartMachineConfigCategory>().Get(4001).InnerIP != "10.1.0.1")
            {
                return Fail(14, "fiber should read mocked start machine config data");
            }

            if (!fiber.RemoveSingleton<StartMachineConfigCategory>())
            {
                return Fail(15, "fiber should remove start machine config mock singleton");
            }

            if (!ReferenceEquals(fiber.GetSingleton<StartMachineConfigCategory>(), global))
            {
                return Fail(16, "fiber should fall back to global start machine config singleton");
            }

            return ErrorCode.ERR_Success;
        }

        private static int VerifyStartProcessConfigCategory(Fiber fiber, List<Action> cleanup)
        {
            StartProcessConfigCategory global = EnsureWorldSingleton(
                fiber,
                cleanup,
                () => FiberConfigSingletonMockFactory.CreateStartProcessConfigCategory(5001, 4001, 10001, "global-process"));
            StartProcessConfigCategory mock = FiberConfigSingletonMockFactory.CreateStartProcessConfigCategory(5001, 4001, 11001, "fiber-process");

            fiber.AddSingleton(mock);
            if (!ReferenceEquals(fiber.GetSingleton<StartProcessConfigCategory>(), mock))
            {
                return Fail(17, "fiber should return start process config mock singleton");
            }

            if (fiber.GetSingleton<StartProcessConfigCategory>().Get(5001).Port != 11001)
            {
                return Fail(18, "fiber should read mocked start process config data");
            }

            if (!fiber.RemoveSingleton<StartProcessConfigCategory>())
            {
                return Fail(19, "fiber should remove start process config mock singleton");
            }

            if (!ReferenceEquals(fiber.GetSingleton<StartProcessConfigCategory>(), global))
            {
                return Fail(20, "fiber should fall back to global start process config singleton");
            }

            return ErrorCode.ERR_Success;
        }

        private static int VerifyStartSceneConfigCategory(Fiber fiber, List<Action> cleanup)
        {
            StartSceneConfigCategory global = EnsureWorldSingleton(
                fiber,
                cleanup,
                () => FiberConfigSingletonMockFactory.CreateStartSceneConfigCategory(
                    6001,
                    5001,
                    1,
                    12001,
                    "GlobalScene",
                    nameof(SceneType.TestCase)));
            StartSceneConfigCategory mock = FiberConfigSingletonMockFactory.CreateStartSceneConfigCategory(
                6001,
                5001,
                1,
                13001,
                "FiberScene",
                nameof(SceneType.TestCase));

            fiber.AddSingleton(mock);
            if (!ReferenceEquals(fiber.GetSingleton<StartSceneConfigCategory>(), mock))
            {
                return Fail(21, "fiber should return start scene config mock singleton");
            }

            StartSceneConfig startSceneConfig = fiber.GetSingleton<StartSceneConfigCategory>().Get(6001);
            if (startSceneConfig.Port != 13001)
            {
                return Fail(22, "fiber should read mocked start scene config data");
            }

            if (fiber.GetSingleton<StartSceneConfigCategory>().GetByProcess(5001).Count != 1)
            {
                return Fail(23, "mocked start scene category should build process index");
            }

            if (fiber.GetSingleton<StartSceneConfigCategory>().GetBySceneName("FiberScene") == null)
            {
                return Fail(24, "mocked start scene category should build scene-name index");
            }

            if (!fiber.RemoveSingleton<StartSceneConfigCategory>())
            {
                return Fail(25, "fiber should remove start scene config mock singleton");
            }

            if (!ReferenceEquals(fiber.GetSingleton<StartSceneConfigCategory>(), global))
            {
                return Fail(26, "fiber should fall back to global start scene config singleton");
            }

            return ErrorCode.ERR_Success;
        }

        private static int VerifyStartZoneConfigCategory(Fiber fiber, List<Action> cleanup)
        {
            StartZoneConfigCategory global = EnsureWorldSingleton(
                fiber,
                cleanup,
                () => FiberConfigSingletonMockFactory.CreateStartZoneConfigCategory(7001, 1, "mongodb://global", "global-db"));
            StartZoneConfigCategory mock = FiberConfigSingletonMockFactory.CreateStartZoneConfigCategory(7001, 2, "mongodb://fiber", "fiber-db");

            fiber.AddSingleton(mock);
            if (!ReferenceEquals(fiber.GetSingleton<StartZoneConfigCategory>(), mock))
            {
                return Fail(27, "fiber should return start zone config mock singleton");
            }

            StartZoneConfig startZoneConfig = fiber.GetSingleton<StartZoneConfigCategory>().Get(7001);
            if (startZoneConfig.DBName != "fiber-db")
            {
                return Fail(28, "fiber should read mocked start zone config data");
            }

            if (!fiber.RemoveSingleton<StartZoneConfigCategory>())
            {
                return Fail(29, "fiber should remove start zone config mock singleton");
            }

            if (!ReferenceEquals(fiber.GetSingleton<StartZoneConfigCategory>(), global))
            {
                return Fail(30, "fiber should fall back to global start zone config singleton");
            }

            return ErrorCode.ERR_Success;
        }

        private static T EnsureWorldSingleton<T>(Fiber fiber, List<Action> cleanup, Func<T> factory) where T : Singleton<T>
        {
            T singleton = fiber.GetSingleton<T>();
            if (singleton != null)
            {
                return singleton;
            }

            T created = factory();
            World.Instance.AddSingleton(created);
            cleanup.Add(World.Instance.RemoveSingleton<T>);
            return created;
        }

        private static int Fail(int code, string message)
        {
            Log.Console(message);
            return code;
        }
    }
}
