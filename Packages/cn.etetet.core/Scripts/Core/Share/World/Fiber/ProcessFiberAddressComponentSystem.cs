namespace ET
{
    [EntitySystemOf(typeof(ProcessFiberAddressComponent))]
    public static partial class ProcessFiberAddressComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ProcessFiberAddressComponent self)
        {
            Scene root = self.Root();
            self.SceneType = root.SceneType;
            self.FiberInstanceId = new FiberInstanceId(root.Fiber.Id);
            self.GetSingleton<ProcessFiberAddressSingleton>().Register(self.SceneType, self.FiberInstanceId);
        }

        [EntitySystem]
        private static void Destroy(this ProcessFiberAddressComponent self)
        {
            self.GetSingleton<ProcessFiberAddressSingleton>().Unregister(self.SceneType, self.FiberInstanceId);
        }
    }
}
