using System;
using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 服务信息系统
    /// </summary>
    [EntitySystemOf(typeof(ServiceInfo))]
    public static partial class ServiceInfoSystem
    {
        [EntitySystem]
        private static void Awake(this ServiceInfo self, string sceneName, ActorId actorId)
        {
            self.SceneName = sceneName;
            self.ActorId = actorId;
            self.Metadata.Clear();
            self.RegisterTime = self.GetSingleton<TimeInfo>().ServerNow();
            self.LastHeartbeatTime = self.RegisterTime;
        }

        [EntitySystem]
        private static void Destroy(this ServiceInfo self)
        {
        }

        /// <summary>
        /// 检查元数据是否匹配过滤条件
        /// 支持多值匹配，过滤条件的值可以用逗号分割，如 "Gate,Realm"
        /// </summary>
        public static bool MatchesFilter(this ServiceInfo self, StringKV filter)
        {
            return ServiceDiscoveryHelper.MatchesMetadataFilter(self.Metadata, filter);
        }

        /// <summary>
        /// 转换为Proto格式
        /// </summary>
        public static ServiceInfoProto ToProto(this ServiceInfo self)
        {
            return ServiceDiscoveryHelper.CreateServiceInfoProto(self.SceneName, self.ActorId, self.Metadata);
        }
    }
}
