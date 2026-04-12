using System;
using ET.Server;

namespace ET.Test
{
    [Invoke(SceneType.Test)]
    public class FiberInit_Test: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Fiber fiber = fiberInit.Fiber;
            Scene root = fiber.Root;
            
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ObjectWait>();
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<Server.ConsoleComponent>();

            EnsureGlobalConfigSingletons(fiber);
            World.Instance.AddSingleton<AddressSingleton>();
            World.Instance.AddSingleton<TestDispatcher>();
            root.AddComponent<TestZoneAllocatorComponent>();
            EnsureAddressSingletonReady(fiber);

            await ETTask.CompletedTask;
        }

        private static void EnsureGlobalConfigSingletons(Fiber fiber)
        {
            EnsureStartMachineConfigCategory(fiber);
            EnsureStartProcessConfigCategory(fiber);
            EnsureStartSceneConfigCategory(fiber);
            EnsureStartZoneConfigCategory(fiber);
        }

        private static void EnsureStartMachineConfigCategory(Fiber fiber)
        {
            if (World.Instance.TryGetSingleton<StartMachineConfigCategory>(out _))
            {
                return;
            }

            StartMachineConfigCategory category = fiber.GetSingleton<StartMachineConfigCategory>();
            if (category != null)
            {
                World.Instance.AddSingleton(category);
                return;
            }
            
            throw new Exception("StartMachineConfigCategory is not initialized before Test scene start.");
        }

        private static void EnsureStartProcessConfigCategory(Fiber fiber)
        {
            if (World.Instance.TryGetSingleton<StartProcessConfigCategory>(out _))
            {
                return;
            }

            StartProcessConfigCategory category = fiber.GetSingleton<StartProcessConfigCategory>();
            if (category != null)
            {
                World.Instance.AddSingleton(category);
                return;
            }
            
            throw new Exception("StartProcessConfigCategory is not initialized before Test scene start.");
        }

        private static void EnsureStartSceneConfigCategory(Fiber fiber)
        {
            if (World.Instance.TryGetSingleton<StartSceneConfigCategory>(out _))
            {
                return;
            }

            StartSceneConfigCategory category = fiber.GetSingleton<StartSceneConfigCategory>();
            if (category != null)
            {
                World.Instance.AddSingleton(category);
                return;
            }
            
            throw new Exception("StartSceneConfigCategory is not initialized before Test scene start.");
        }

        private static void EnsureStartZoneConfigCategory(Fiber fiber)
        {
            if (World.Instance.TryGetSingleton<StartZoneConfigCategory>(out _))
            {
                return;
            }

            StartZoneConfigCategory category = fiber.GetSingleton<StartZoneConfigCategory>();
            if (category != null)
            {
                World.Instance.AddSingleton(category);
                return;
            }
            
            throw new Exception("StartZoneConfigCategory is not initialized before Test scene start.");
        }

        private static void EnsureAddressSingletonReady(Fiber fiber)
        {
            AddressSingleton addressSingleton = AddressSingleton.Instance;
            if (!string.IsNullOrEmpty(addressSingleton.InnerIP) &&
                !string.IsNullOrEmpty(addressSingleton.OuterIP) &&
                addressSingleton.InnerPort > 0)
            {
                return;
            }

            StartProcessConfig startProcessConfig = fiber.GetSingleton<StartProcessConfigCategory>()?.Get(Options.Instance.Process);
            if (startProcessConfig == null)
            {
                throw new Exception($"test address init failed: process={Options.Instance.Process}");
            }

            StartMachineConfig startMachineConfig = fiber.GetSingleton<StartMachineConfigCategory>()?.Get(startProcessConfig.MachineId);
            addressSingleton.InnerIP ??= startMachineConfig?.InnerIP;
            addressSingleton.OuterIP ??= startMachineConfig?.OuterIP;
            addressSingleton.InnerPort = addressSingleton.InnerPort > 0 ? addressSingleton.InnerPort : startProcessConfig.Port;
        }
    }
}
