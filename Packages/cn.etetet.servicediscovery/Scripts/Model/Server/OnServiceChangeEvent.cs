using System.Collections.Generic;

namespace ET.Server
{
    public struct OnServiceChangeAddService
    {
        public string ServiceName;
    }

    public struct OnServiceChangeRemoveService
    {
        public string ServiceName;
    }

    [ComponentOf(typeof(Scene))]
    public class ServiceDiscoveryTestAddEventCounterComponent : Entity, IAwake
    {
        public Dictionary<string, int> Counts = new();
    }
}
