namespace ET
{
    [EnableClass]
    public sealed class UnityBridgeDeferredCommandState : Object
    {
        public string CommandJson { get; set; }

        public int RpcId { get; set; }

        public string IdempotencyKey { get; set; }

        public int TimeoutMs { get; set; }

        public long StartedAt { get; set; }

    }
}
