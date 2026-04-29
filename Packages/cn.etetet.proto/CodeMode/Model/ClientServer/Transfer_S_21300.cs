using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(Opcode.G2Map_EnterMap)]
    [ResponseType(nameof(Map2G_EnterMap))]
    public partial class G2Map_EnterMap : MessageObject, IRequest
    {
        public static G2Map_EnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2Map_EnterMap>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public long PlayerId { get; set; }
        [MemoryPackOrder(2)]
        public ActorId GateActorId { get; set; }
        [MemoryPackOrder(3)]
        public string MapName { get; set; }
        [MemoryPackOrder(4)]
        public long MapId { get; set; }
        [MemoryPackOrder(5)]
        public int UnitConfigId { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PlayerId = default;
            this.GateActorId = default;
            this.MapName = default;
            this.MapId = default;
            this.UnitConfigId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.Map2G_EnterMap)]
    public partial class Map2G_EnterMap : MessageObject, IResponse
    {
        public static Map2G_EnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Map2G_EnterMap>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2M_UnitTransferRequest)]
    [ResponseType(nameof(M2M_UnitTransferResponse))]
    public partial class M2M_UnitTransferRequest : MessageObject, IRequest, IEntityMessage
    {
        public static M2M_UnitTransferRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2M_UnitTransferRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public ActorId OldActorId { get; set; }
        [MemoryPackOrder(2)]
        public byte[] UnitBytes { get; set; }
        [MemoryPackOrder(3)]
        public List<byte[]> EntityBytes { get; set; } = new();

        [MemoryPackOrder(4)]
        public bool ChangeScene { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.OldActorId = default;
            this.UnitBytes = default;
            this.EntityBytes.Clear();
            this.ChangeScene = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2M_UnitTransferResponse)]
    public partial class M2M_UnitTransferResponse : MessageObject, IResponse
    {
        public static M2M_UnitTransferResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2M_UnitTransferResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public ActorId NewActorId { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.NewActorId = default;

            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort G2Map_EnterMap = 21301;
        public const ushort Map2G_EnterMap = 21302;
        public const ushort M2M_UnitTransferRequest = 21303;
        public const ushort M2M_UnitTransferResponse = 21304;
    }
}