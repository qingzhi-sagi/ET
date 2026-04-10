using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(Opcode.TestEcho)]
    [ResponseType(nameof(TestEchoResponse))]
    public partial class TestEcho : MessageObject, IRequest
    {
        public static TestEcho Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TestEcho>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Text { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Text = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TestEchoResponse)]
    public partial class TestEchoResponse : MessageObject, IResponse
    {
        public static TestEchoResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TestEchoResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string Text { get; set; }
        [MemoryPackOrder(4)]
        public long HandledAt { get; set; }
        [MemoryPackOrder(5)]
        public string Handler { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Text = default;
            this.HandledAt = default;
            this.Handler = default;

            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort TestEcho = 11201;
        public const ushort TestEchoResponse = 11202;
    }
}