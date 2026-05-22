using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(Opcode.A2MapManager_GetMapRequest)]
    [ResponseType(nameof(A2MapManager_GetMapResponse))]
    public partial class A2MapManager_GetMapRequest : MessageObject, IRequest
    {
        public static A2MapManager_GetMapRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<A2MapManager_GetMapRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string MapName { get; set; }
        [MemoryPackOrder(2)]
        public long MapId { get; set; }
        [MemoryPackOrder(3)]
        public long UnitId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.A2MapManager_GetMapResponse)]
    public partial class A2MapManager_GetMapResponse : MessageObject, IResponse
    {
        public static A2MapManager_GetMapResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<A2MapManager_GetMapResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string MapName { get; set; }
        [MemoryPackOrder(4)]
        public long MapId { get; set; }
        [MemoryPackOrder(5)]
        public ActorId MapActorId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.A2MapManager_NotifyPlayerAlreadyEnterMapRequest)]
    [ResponseType(nameof(A2MapManager_NotifyPlayerAlreadyEnterMapResponse))]
    public partial class A2MapManager_NotifyPlayerAlreadyEnterMapRequest : MessageObject, IRequest
    {
        public static A2MapManager_NotifyPlayerAlreadyEnterMapRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<A2MapManager_NotifyPlayerAlreadyEnterMapRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string MapName { get; set; }
        [MemoryPackOrder(2)]
        public long MapId { get; set; }
        [MemoryPackOrder(3)]
        public long UnitId { get; set; }
        [MemoryPackOrder(4)]
        public long PreMapCopyId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.A2MapManager_NotifyPlayerAlreadyEnterMapResponse)]
    public partial class A2MapManager_NotifyPlayerAlreadyEnterMapResponse : MessageObject, IResponse
    {
        public static A2MapManager_NotifyPlayerAlreadyEnterMapResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<A2MapManager_NotifyPlayerAlreadyEnterMapResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.MapManager2Map_NotifyPlayerTransferRequest)]
    [ResponseType(nameof(MapManager2Map_NotifyPlayerTransferResponse))]
    public partial class MapManager2Map_NotifyPlayerTransferRequest : MessageObject, ILocationRequest
    {
        public static MapManager2Map_NotifyPlayerTransferRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<MapManager2Map_NotifyPlayerTransferRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string MapName { get; set; }
        [MemoryPackOrder(2)]
        public long MapId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.MapManager2Map_NotifyPlayerTransferResponse)]
    public partial class MapManager2Map_NotifyPlayerTransferResponse : MessageObject, ILocationResponse
    {
        public static MapManager2Map_NotifyPlayerTransferResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<MapManager2Map_NotifyPlayerTransferResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.Map2MapManager_LogoutRequest)]
    [ResponseType(nameof(Map2MapManager_LogoutResponse))]
    public partial class Map2MapManager_LogoutRequest : MessageObject, IRequest
    {
        public static Map2MapManager_LogoutRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Map2MapManager_LogoutRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string MapName { get; set; }
        [MemoryPackOrder(2)]
        public long UnitId { get; set; }
        [MemoryPackOrder(3)]
        public long MapId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.Map2MapManager_LogoutResponse)]
    public partial class Map2MapManager_LogoutResponse : MessageObject, IResponse
    {
        public static Map2MapManager_LogoutResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Map2MapManager_LogoutResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort A2MapManager_GetMapRequest = 21001;
        public const ushort A2MapManager_GetMapResponse = 21002;
        public const ushort A2MapManager_NotifyPlayerAlreadyEnterMapRequest = 21003;
        public const ushort A2MapManager_NotifyPlayerAlreadyEnterMapResponse = 21004;
        public const ushort MapManager2Map_NotifyPlayerTransferRequest = 21005;
        public const ushort MapManager2Map_NotifyPlayerTransferResponse = 21006;
        public const ushort Map2MapManager_LogoutRequest = 21007;
        public const ushort Map2MapManager_LogoutResponse = 21008;
    }
}