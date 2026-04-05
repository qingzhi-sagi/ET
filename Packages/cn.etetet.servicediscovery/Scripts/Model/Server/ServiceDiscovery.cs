using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 服务发现组件，挂载到Fiber上成为服务发现Fiber
    /// 管理所有注册的服务信息，并向Agent广播服务变更
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ServiceDiscovery : Entity, IAwake, IDestroy, IUpdate
    {
        /// <summary>
        /// 所有注册的服务信息，Key为服务的唯一标识SceneName
        /// </summary>
        public Dictionary<string, EntityRef<ServiceInfo>> Services = new();

        /// <summary>
        /// 服务索引，key 为索引名，value 为索引值到 SceneName 的映射。
        /// </summary>
        public Dictionary<string, MultiMapSet<string, string>> ServicesIndexs = new();

        /// <summary>
        /// Agent 路由表（按进程地址路由到ServiceDiscoveryAgent ActorId）。
        /// </summary>
        public Dictionary<Address, ActorId> AgentActorIds = new();

        /// <summary>
        /// 记录每个 Agent 最近一次注册重放声明拥有的服务名集合。
        /// 仅用于后续重注册时按“上次已声明拥有的服务”做收敛，避免误删同地址下的其他服务。
        /// </summary>
        public Dictionary<Address, HashSet<string>> AgentOwnedSceneNames = new();

    }
}
