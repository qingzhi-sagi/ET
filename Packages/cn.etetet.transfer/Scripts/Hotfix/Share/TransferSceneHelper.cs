namespace ET
{
    public static class TransferSceneHelper
    {
        public static bool IsChangeScene(Fiber fiber, string preSceneName, string nextSceneName)
        {
            MapConfig preMapConfig = null;
            if (preSceneName != null)
            {
                preMapConfig = fiber.GetSingleton<MapConfigCategory>().GetByName(preSceneName.GetSceneConfigName());
            }
            MapConfig mapConfig = fiber.GetSingleton<MapConfigCategory>().GetByName(nextSceneName.GetSceneConfigName());
            return preMapConfig?.MapResName != mapConfig?.MapResName;
        }
    }
}
