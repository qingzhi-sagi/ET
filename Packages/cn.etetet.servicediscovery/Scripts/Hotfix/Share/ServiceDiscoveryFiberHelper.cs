namespace ET
{
    public static class ServiceDiscoveryFiberHelper
    {
        public static int GetAgentFiberId(int zone)
        {
            return unchecked((int)(((uint)zone << FiberIdHelper.LocalSlotBits) | (uint)Const.ServiceDiscoveryAgentFiberId));
        }

        public static FiberInstanceId CreateAgentFiberInstanceId(int zone)
        {
            return new FiberInstanceId(GetAgentFiberId(zone));
        }
    }
}
