using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(Opcode.MoveInfo)]
    public partial class MoveInfo : MessageObject
    {
        public static MoveInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<MoveInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public List<Unity.Mathematics.float3> Points { get; set; } = new();

        [MemoryPackOrder(1)]
        public Unity.Mathematics.quaternion Rotation { get; set; }
        [MemoryPackOrder(2)]
        public int TurnSpeed { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Points.Clear();
            this.Rotation = default;
            this.TurnSpeed = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.C2M_PathfindingResult)]
    public partial class C2M_PathfindingResult : MessageObject, ILocationMessage
    {
        public static C2M_PathfindingResult Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_PathfindingResult>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public Unity.Mathematics.float3 Position { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Position = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.C2M_Stop)]
    public partial class C2M_Stop : MessageObject, ILocationMessage
    {
        public static C2M_Stop Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_Stop>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2C_PathfindingResult)]
    public partial class M2C_PathfindingResult : MessageObject, ICurrentMessage
    {
        public static M2C_PathfindingResult Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_PathfindingResult>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public long Id { get; set; }
        [MemoryPackOrder(1)]
        public Unity.Mathematics.float3 Position { get; set; }
        [MemoryPackOrder(2)]
        public List<Unity.Mathematics.float3> Points { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Id = default;
            this.Position = default;
            this.Points.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2C_Stop)]
    public partial class M2C_Stop : MessageObject, ICurrentMessage
    {
        public static M2C_Stop Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_Stop>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int Error { get; set; }
        [MemoryPackOrder(1)]
        public long Id { get; set; }
        [MemoryPackOrder(2)]
        public Unity.Mathematics.float3 Position { get; set; }
        [MemoryPackOrder(3)]
        public Unity.Mathematics.quaternion Rotation { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Error = default;
            this.Id = default;
            this.Position = default;
            this.Rotation = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2C_Turn)]
    public partial class M2C_Turn : MessageObject, ICurrentMessage
    {
        public static M2C_Turn Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_Turn>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public long UnitId { get; set; }
        [MemoryPackOrder(1)]
        public Unity.Mathematics.quaternion Rotation { get; set; }
        [MemoryPackOrder(2)]
        public int TurnTime { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.UnitId = default;
            this.Rotation = default;
            this.TurnTime = default;

            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort MoveInfo = 10301;
        public const ushort C2M_PathfindingResult = 10302;
        public const ushort C2M_Stop = 10303;
        public const ushort M2C_PathfindingResult = 10304;
        public const ushort M2C_Stop = 10305;
        public const ushort M2C_Turn = 10306;
    }
}