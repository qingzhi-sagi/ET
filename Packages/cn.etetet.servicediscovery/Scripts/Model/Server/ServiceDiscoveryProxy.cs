using System;
using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 服务发现代理组件，挂载在Fiber上提供服务注册/订阅/查询入口
    /// 通过此组件统一转发到进程级 ServiceDiscoveryAgent
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public partial class ServiceDiscoveryProxy : Entity, IAwake, IDestroy
    {
        // 这里可以指定索引
        public readonly string[] Indexs = { ServiceMetaKey.SceneType };
        
        /// <summary>
        /// 进程级 ServiceDiscoveryAgent 的 FiberInstanceId。
        /// </summary>
        public FiberInstanceId AgentFiberInstanceId;

        /// <summary>
        /// 缓存的服务列表，Key为SceneType，Value为该类型的服务SceneName列表
        /// </summary>
        public Dictionary<string, EntityRef<ServiceInfo>> SceneNameServices = new();
        //  索引,key1例如"SceneType",  key2 是值，例如SceneType的值，  HashSet中是ServiceCacheInfo的SceneName
        public Dictionary<string, MultiMapSet<string, string>> ServicesIndexs = new();

        /// <summary>
        /// 订阅过滤条件缓存（用于本地保留调用参数）。
        /// </summary>
        public Dictionary<string, StringKV> SubscribeFilters = new();

        private EntityRef<ProcessInnerSender> processInnerSender;

        public string RootName;

        public int ServiceResolveRetryTimes;

        public int ServiceResolveRetryIntervalMs;

        public ProcessInnerSender ProcessInnerSender
        {
            get
            {
                return this.processInnerSender;
            }
            set
            {
                this.processInnerSender = value;
            }
        }
    }

}
