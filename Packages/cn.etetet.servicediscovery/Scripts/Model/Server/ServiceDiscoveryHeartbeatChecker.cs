namespace ET.Server
{
    /// <summary>
    /// 主服务发现节点的 Agent 心跳超时检测组件。
    /// </summary>
    [ComponentOf(typeof(ServiceDiscovery))]
    public class ServiceDiscoveryHeartbeatChecker : Entity, IAwake, IDestroy, IUpdate
    {
        /// <summary>
        /// 上次检查时间（毫秒时间戳）。
        /// </summary>
        public long LastCheckTime;
    }
}
