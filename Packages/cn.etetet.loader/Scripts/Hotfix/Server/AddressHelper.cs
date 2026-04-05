namespace ET.Server
{
    public static class AddressHelper
    {
        public static void SetInnerIPInnerPortOuterIP(this AddressSingleton addressSingleton, StartProcessConfig startProcessConfig)
        {
            StartMachineConfigCategory machineConfigCategory = World.Instance.GetSingleton<StartMachineConfigCategory>();
            StartMachineConfig startMachineConfig = machineConfigCategory?.Get(startProcessConfig.MachineId);
            addressSingleton.InnerIP ??= startMachineConfig?.InnerIP;
            addressSingleton.OuterIP ??= startMachineConfig?.OuterIP;
            addressSingleton.InnerPort = addressSingleton.InnerPort > 0 ? addressSingleton.InnerPort : startProcessConfig.Port;
        }

        public static void SetInnerIPInnerPortOuterIP(this AddressSingleton addressSingleton, Fiber fiber, StartProcessConfig startProcessConfig)
        {
            addressSingleton.InnerIP ??= startProcessConfig.GetInnerIP(fiber);
            addressSingleton.OuterIP ??= startProcessConfig.GetOuterIP(fiber);
            addressSingleton.InnerPort = addressSingleton.InnerPort > 0 ? addressSingleton.InnerPort : startProcessConfig.Port;
        }
        
        // 注意一个进程中会有多个OuterPort,所以不能只使用AddressSingleton.Instance.OuterPort
        // 如果AddressSingleton.Instance.OuterPort大于0，说明aspire有传入port，那么说明进程中只有一个Scene,并且配置了具体的port，可以直接使用
        // 否则，必须从sceneconfig中获取
        public static int GetSceneOuterPort(this AddressSingleton addressSingleton, Fiber fiber, string sceneName)
        {
            int outerPort = addressSingleton.OuterPort;

            if (outerPort > 0)
            {
                return outerPort;
            }

            StartSceneConfig startSceneConfig = fiber.GetSingleton<StartSceneConfigCategory>().GetBySceneName(sceneName);
            return startSceneConfig.Port;
        }
    }
}
