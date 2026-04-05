namespace ET.Server
{
    /// <summary>
    /// Agent -> 主服务发现心跳组件。
    /// </summary>
    [ComponentOf(typeof(ServiceDiscoveryAgent))]
    public class ServiceDiscoveryAgentMasterHeartbeat : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 心跳发送间隔（毫秒）。
        /// </summary>
        public long Interval = 2 * 1000;

        /// <summary>
        /// 单次主心跳 RPC 超时时间（毫秒）。
        /// 到时即视为心跳失败并触发主地址重解析。
        /// </summary>
        public long RpcTimeout = 5 * 1000;

        /// <summary>
        /// 连续失败次数。
        /// </summary>
        public int FailureCount;

        /// <summary>
        /// 断路器开启截止时间（毫秒时间戳）。
        /// </summary>
        public long CircuitOpenUntil;

        /// <summary>
        /// 下次允许重试时间（毫秒时间戳）。
        /// </summary>
        public long NextRetryTime;

        /// <summary>
        /// 连续失败阈值。
        /// </summary>
        public int CircuitThreshold = 5;

        /// <summary>
        /// 断路器开启时长（毫秒）。
        /// </summary>
        public long CircuitOpenDuration = 3 * 1000;

        /// <summary>
        /// 失败退避基础时长（毫秒）。
        /// </summary>
        public long RetryBackoffBase = 100;

        /// <summary>
        /// 失败退避上限（毫秒）。
        /// </summary>
        public long RetryBackoffMax = 2 * 1000;

        /// <summary>
        /// 原始时钟上次采样值（用于回拨检测）。
        /// </summary>
        public long LastRawNow;

        /// <summary>
        /// 防回退单调时钟。
        /// </summary>
        public long MonotonicNow;
    }
}
