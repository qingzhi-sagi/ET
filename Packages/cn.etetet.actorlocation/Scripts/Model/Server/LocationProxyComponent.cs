using System;
using System.Collections.Generic;

namespace ET.Server
{
    public struct LocationLockTokenInfo
    {
        public long LockToken;
    }

    [ComponentOf(typeof(Scene))]
    public class LocationProxyComponent: Entity, IAwake
    {
        public string primaryLocationSceneName;

        public ulong primaryLocationPriorityId;

        public int locationRequestRetryTimes;

        public int locationRequestRetryIntervalMs;
    }
}
