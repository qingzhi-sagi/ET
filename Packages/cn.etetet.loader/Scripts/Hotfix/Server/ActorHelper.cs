namespace ET.Server
{
    public static class ActorHelper
    {
        public static ActorId GetActorId(this Entity self)
        {
            Fiber root = self.Fiber();
            return new ActorId(self.GetSingleton<AddressSingleton>().InnerAddress, new FiberInstanceId(root.Id, self.InstanceId));
        }
    }
}
