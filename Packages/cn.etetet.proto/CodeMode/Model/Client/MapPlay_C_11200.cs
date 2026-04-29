using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(Opcode.C2G_EnterMap)]
    [ResponseType(nameof(G2C_EnterMap))]
    public partial class C2G_EnterMap : MessageObject, ISessionRequest
    {
        public static C2G_EnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2G_EnterMap>(isFromPool);
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
    [Message(Opcode.G2C_EnterMap)]
    public partial class G2C_EnterMap : MessageObject, ISessionResponse
    {
        public static G2C_EnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2C_EnterMap>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        /// <summary>
        /// 自己的UnitId
        /// </summary>
        [MemoryPackOrder(3)]
        public long MyId { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.MyId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.PetInfo)]
    public partial class PetInfo : MessageObject
    {
        public static PetInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PetInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public long OwnerId { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.OwnerId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.UnitInfo)]
    public partial class UnitInfo : MessageObject
    {
        public static UnitInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<UnitInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public long UnitId { get; set; }
        [MemoryPackOrder(1)]
        public int ConfigId { get; set; }
        [MemoryPackOrder(2)]
        public int Type { get; set; }
        [MemoryPackOrder(3)]
        public Unity.Mathematics.float3 Position { get; set; }
        [MemoryPackOrder(4)]
        public Unity.Mathematics.float3 Forward { get; set; }
        [MongoDB.Bson.Serialization.Attributes.BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]
        [MemoryPackOrder(5)]
        public Dictionary<int, long> KV { get; set; } = new();
        [MemoryPackOrder(6)]
        public MoveInfo MoveInfo { get; set; }
        [MemoryPackOrder(7)]
        public PetInfo PetInfo { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.UnitId = default;
            this.ConfigId = default;
            this.Type = default;
            this.Position = default;
            this.Forward = default;
            this.KV.Clear();
            this.MoveInfo = default;
            this.PetInfo = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2C_CreateUnits)]
    public partial class M2C_CreateUnits : MessageObject, ICurrentMessage
    {
        public static M2C_CreateUnits Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_CreateUnits>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public List<UnitInfo> Units { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Units.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2C_CreateMyUnit)]
    public partial class M2C_CreateMyUnit : MessageObject, ICurrentMessage
    {
        public static M2C_CreateMyUnit Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_CreateMyUnit>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public UnitInfo Unit { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Unit = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2C_StartSceneChange)]
    public partial class M2C_StartSceneChange : MessageObject, IMessage
    {
        public static M2C_StartSceneChange Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_StartSceneChange>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public long SceneId { get; set; }
        [MemoryPackOrder(1)]
        public string SceneName { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.SceneId = default;
            this.SceneName = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2C_RemoveUnits)]
    public partial class M2C_RemoveUnits : MessageObject, ICurrentMessage
    {
        public static M2C_RemoveUnits Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_RemoveUnits>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public List<long> Units { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Units.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2C_Error)]
    public partial class M2C_Error : MessageObject, IMessage
    {
        public static M2C_Error Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_Error>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int Error { get; set; }
        [MemoryPackOrder(1)]
        public List<string> Values { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Error = default;
            this.Values.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2C_NumericChange)]
    public partial class M2C_NumericChange : MessageObject, ICurrentMessage
    {
        public static M2C_NumericChange Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_NumericChange>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public long UnitId { get; set; }
        [MemoryPackOrder(1)]
        public int NumericType { get; set; }
        [MemoryPackOrder(2)]
        public long Value { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.UnitId = default;
            this.NumericType = default;
            this.Value = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.C2M_TransferMap)]
    [ResponseType(nameof(M2C_TransferMap))]
    public partial class C2M_TransferMap : MessageObject, ILocationRequest
    {
        public static C2M_TransferMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_TransferMap>(isFromPool);
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
    [Message(Opcode.M2C_TransferMap)]
    public partial class M2C_TransferMap : MessageObject, ILocationResponse
    {
        public static M2C_TransferMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_TransferMap>(isFromPool);
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
    [Message(Opcode.C2M_PetAttack)]
    public partial class C2M_PetAttack : MessageObject, ILocationMessage
    {
        public static C2M_PetAttack Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_PetAttack>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public long UnitId { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.UnitId = default;

            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort C2G_EnterMap = 11201;
        public const ushort G2C_EnterMap = 11202;
        public const ushort PetInfo = 11203;
        public const ushort UnitInfo = 11204;
        public const ushort M2C_CreateUnits = 11205;
        public const ushort M2C_CreateMyUnit = 11206;
        public const ushort M2C_StartSceneChange = 11207;
        public const ushort M2C_RemoveUnits = 11208;
        public const ushort M2C_Error = 11209;
        public const ushort M2C_NumericChange = 11210;
        public const ushort C2M_TransferMap = 11211;
        public const ushort M2C_TransferMap = 11212;
        public const ushort C2M_PetAttack = 11213;
    }
}