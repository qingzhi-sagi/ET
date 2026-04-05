using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// Agent 侧 Proxy 心跳监控组件。
    /// </summary>
    [ComponentOf(typeof(ServiceDiscoveryAgent))]
    public class ServiceDiscoveryAgentProxyHeartbeat : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 心跳检查间隔（毫秒）。
        /// </summary>
        public long CheckInterval = 2 * 1000;

        /// <summary>
        /// Proxy 心跳超时时间（毫秒），超时后由 Agent 触发注销。
        /// </summary>
        public long Timeout = 10 * 1000;

        /// <summary>
        /// 心跳检查定时器句柄。
        /// </summary>
        public long Timer;

        /// <summary>
        /// 本进程 Proxy 心跳时间（Key: Proxy SceneName）。
        /// </summary>
        public Dictionary<string, long> HeartbeatTimes = new();

        /// <summary>
        /// 避免并发执行超时检查。
        /// </summary>
        public bool Checking;

        /// <summary>
        /// 超时注销连续失败次数（按场景）。
        /// </summary>
        public Dictionary<string, int> TimeoutUnregisterFailureCounts = new();

        /// <summary>
        /// 超时注销下次可重试时间（按场景，毫秒时间戳）。
        /// </summary>
        public Dictionary<string, long> TimeoutUnregisterNextRetryTimes = new();

        /// <summary>
        /// 超时注销失败退避基础时长（毫秒）。
        /// </summary>
        public long TimeoutUnregisterBackoffBase = 200;

        /// <summary>
        /// 超时注销失败退避上限（毫秒）。
        /// </summary>
        public long TimeoutUnregisterBackoffMax = 5 * 1000;

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
