using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(ServiceDiscovery))]
    public class ServiceDiscoveryNotificationBufferComponent : Entity, IAwake
    {
        public long NotificationDebounceInterval = 100;

        public int NotificationBatchMaxItems = 128;

        public long LastNotificationFlushTime;

        public Dictionary<ActorId, Dictionary<string, (int ChangeType, string SceneName, ActorId ActorId, StringKV Metadata)>>
            PendingNotifications = new();
    }
}
