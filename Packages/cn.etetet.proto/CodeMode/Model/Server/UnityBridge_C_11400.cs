using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(Opcode.GameObjectCreateRequest)]
    [ResponseType(nameof(GameObjectCreateResponse))]
    public partial class GameObjectCreateRequest : MessageObject, IRequest
    {
        public static GameObjectCreateRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectCreateRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Name { get; set; }
        [MemoryPackOrder(2)]
        public string PrimitiveType { get; set; }
        [MemoryPackOrder(3)]
        public string ParentPath { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameObjectCreateResponse)]
    public partial class GameObjectCreateResponse : MessageObject, IResponse
    {
        public static GameObjectCreateResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectCreateResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public bool Created { get; set; }
        [MemoryPackOrder(4)]
        public BridgeObjectInfo Object { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameObjectDestroyRequest)]
    [ResponseType(nameof(GameObjectDestroyResponse))]
    public partial class GameObjectDestroyRequest : MessageObject, IRequest
    {
        public static GameObjectDestroyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectDestroyRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameObjectDestroyResponse)]
    public partial class GameObjectDestroyResponse : MessageObject, IResponse
    {
        public static GameObjectDestroyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectDestroyResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public bool Destroyed { get; set; }
        [MemoryPackOrder(4)]
        public string DestroyedName { get; set; }
        [MemoryPackOrder(5)]
        public string DestroyedPath { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameObjectFindRequest)]
    [ResponseType(nameof(GameObjectFindResponse))]
    public partial class GameObjectFindRequest : MessageObject, IRequest
    {
        public static GameObjectFindRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectFindRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Name { get; set; }
        [MemoryPackOrder(2)]
        public string Tag { get; set; }
        [MemoryPackOrder(3)]
        public string WithComponent { get; set; }
        [MemoryPackOrder(4)]
        public int MaxResults { get; set; }
        [MemoryPackOrder(5)]
        public bool IncludeInactive { get; set; }
        [MemoryPackOrder(6)]
        public bool IncludeComponents { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameObjectFindResponse)]
    public partial class GameObjectFindResponse : MessageObject, IResponse
    {
        public static GameObjectFindResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectFindResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public List<BridgeObjectInfo> Objects { get; set; } = new();

        [MemoryPackOrder(4)]
        public int Count { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameObjectSetActiveRequest)]
    [ResponseType(nameof(GameObjectSetActiveResponse))]
    public partial class GameObjectSetActiveRequest : MessageObject, IRequest
    {
        public static GameObjectSetActiveRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectSetActiveRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public bool Active { get; set; }
        [MemoryPackOrder(4)]
        public bool Toggle { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameObjectSetActiveResponse)]
    public partial class GameObjectSetActiveResponse : MessageObject, IResponse
    {
        public static GameObjectSetActiveResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectSetActiveResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public BridgeObjectInfo Object { get; set; }
        [MemoryPackOrder(4)]
        public bool ActiveSelf { get; set; }
        [MemoryPackOrder(5)]
        public bool ActiveInHierarchy { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameObjectRenameRequest)]
    [ResponseType(nameof(GameObjectRenameResponse))]
    public partial class GameObjectRenameRequest : MessageObject, IRequest
    {
        public static GameObjectRenameRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectRenameRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public string NewName { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameObjectRenameResponse)]
    public partial class GameObjectRenameResponse : MessageObject, IResponse
    {
        public static GameObjectRenameResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectRenameResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string OldName { get; set; }
        [MemoryPackOrder(4)]
        public string NewName { get; set; }
        [MemoryPackOrder(5)]
        public BridgeObjectInfo Object { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameObjectDuplicateRequest)]
    [ResponseType(nameof(GameObjectDuplicateResponse))]
    public partial class GameObjectDuplicateRequest : MessageObject, IRequest
    {
        public static GameObjectDuplicateRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectDuplicateRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameObjectDuplicateResponse)]
    public partial class GameObjectDuplicateResponse : MessageObject, IResponse
    {
        public static GameObjectDuplicateResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectDuplicateResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public BridgeObjectInfo Original { get; set; }
        [MemoryPackOrder(4)]
        public BridgeObjectInfo Duplicate { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameObjectGetInfoRequest)]
    [ResponseType(nameof(GameObjectGetInfoResponse))]
    public partial class GameObjectGetInfoRequest : MessageObject, IRequest
    {
        public static GameObjectGetInfoRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectGetInfoRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public bool IncludeComponents { get; set; }
        [MemoryPackOrder(4)]
        public int MaxChildren { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameObjectGetInfoResponse)]
    public partial class GameObjectGetInfoResponse : MessageObject, IResponse
    {
        public static GameObjectGetInfoResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameObjectGetInfoResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public BridgeObjectInfo Object { get; set; }
        [MemoryPackOrder(4)]
        public int ChildCount { get; set; }
        [MemoryPackOrder(5)]
        public List<string> Children { get; set; } = new();

        [MemoryPackOrder(6)]
        public string ParentName { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformGetRequest)]
    [ResponseType(nameof(TransformGetResponse))]
    public partial class TransformGetRequest : MessageObject, IRequest
    {
        public static TransformGetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformGetRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformGetResponse)]
    public partial class TransformGetResponse : MessageObject, IResponse
    {
        public static TransformGetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformGetResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string Name { get; set; }
        [MemoryPackOrder(4)]
        public string Path { get; set; }
        [MemoryPackOrder(5)]
        public BridgeTransformInfo Transform { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformSetPositionRequest)]
    [ResponseType(nameof(TransformSetPositionResponse))]
    public partial class TransformSetPositionRequest : MessageObject, IRequest
    {
        public static TransformSetPositionRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetPositionRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public BridgeVector3 Position { get; set; }
        [MemoryPackOrder(4)]
        public bool Local { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformSetPositionResponse)]
    public partial class TransformSetPositionResponse : MessageObject, IResponse
    {
        public static TransformSetPositionResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetPositionResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string Name { get; set; }
        [MemoryPackOrder(4)]
        public string Path { get; set; }
        [MemoryPackOrder(5)]
        public BridgeTransformInfo Transform { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformSetRotationRequest)]
    [ResponseType(nameof(TransformSetRotationResponse))]
    public partial class TransformSetRotationRequest : MessageObject, IRequest
    {
        public static TransformSetRotationRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetRotationRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public BridgeVector3 EulerAngles { get; set; }
        [MemoryPackOrder(4)]
        public bool Local { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformSetRotationResponse)]
    public partial class TransformSetRotationResponse : MessageObject, IResponse
    {
        public static TransformSetRotationResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetRotationResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string Name { get; set; }
        [MemoryPackOrder(4)]
        public string Path { get; set; }
        [MemoryPackOrder(5)]
        public BridgeTransformInfo Transform { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformSetScaleRequest)]
    [ResponseType(nameof(TransformSetScaleResponse))]
    public partial class TransformSetScaleRequest : MessageObject, IRequest
    {
        public static TransformSetScaleRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetScaleRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public BridgeVector3 Scale { get; set; }
        [MemoryPackOrder(4)]
        public bool UseUniform { get; set; }
        [MemoryPackOrder(5)]
        public float Uniform { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformSetScaleResponse)]
    public partial class TransformSetScaleResponse : MessageObject, IResponse
    {
        public static TransformSetScaleResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetScaleResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string Name { get; set; }
        [MemoryPackOrder(4)]
        public string Path { get; set; }
        [MemoryPackOrder(5)]
        public BridgeTransformInfo Transform { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformSetParentRequest)]
    [ResponseType(nameof(TransformSetParentResponse))]
    public partial class TransformSetParentRequest : MessageObject, IRequest
    {
        public static TransformSetParentRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetParentRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public string ParentPath { get; set; }
        [MemoryPackOrder(4)]
        public int ParentInstanceId { get; set; }
        [MemoryPackOrder(5)]
        public bool WorldPositionStays { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformSetParentResponse)]
    public partial class TransformSetParentResponse : MessageObject, IResponse
    {
        public static TransformSetParentResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetParentResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string Name { get; set; }
        [MemoryPackOrder(4)]
        public string Path { get; set; }
        [MemoryPackOrder(5)]
        public string ParentPath { get; set; }
        [MemoryPackOrder(6)]
        public BridgeTransformInfo Transform { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformLookAtRequest)]
    [ResponseType(nameof(TransformLookAtResponse))]
    public partial class TransformLookAtRequest : MessageObject, IRequest
    {
        public static TransformLookAtRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformLookAtRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public string TargetPath { get; set; }
        [MemoryPackOrder(4)]
        public int TargetInstanceId { get; set; }
        [MemoryPackOrder(5)]
        public BridgeVector3 TargetPosition { get; set; }
        [MemoryPackOrder(6)]
        public bool HasTargetPosition { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformLookAtResponse)]
    public partial class TransformLookAtResponse : MessageObject, IResponse
    {
        public static TransformLookAtResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformLookAtResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string Name { get; set; }
        [MemoryPackOrder(4)]
        public string Path { get; set; }
        [MemoryPackOrder(5)]
        public BridgeTransformInfo Transform { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformResetRequest)]
    [ResponseType(nameof(TransformResetResponse))]
    public partial class TransformResetRequest : MessageObject, IRequest
    {
        public static TransformResetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformResetRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public bool ResetPosition { get; set; }
        [MemoryPackOrder(4)]
        public bool ResetRotation { get; set; }
        [MemoryPackOrder(5)]
        public bool ResetScale { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformResetResponse)]
    public partial class TransformResetResponse : MessageObject, IResponse
    {
        public static TransformResetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformResetResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string Name { get; set; }
        [MemoryPackOrder(4)]
        public string Path { get; set; }
        [MemoryPackOrder(5)]
        public BridgeTransformInfo Transform { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformSetSiblingIndexRequest)]
    [ResponseType(nameof(TransformSetSiblingIndexResponse))]
    public partial class TransformSetSiblingIndexRequest : MessageObject, IRequest
    {
        public static TransformSetSiblingIndexRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetSiblingIndexRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public int Index { get; set; }
        [MemoryPackOrder(4)]
        public bool First { get; set; }
        [MemoryPackOrder(5)]
        public bool Last { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.TransformSetSiblingIndexResponse)]
    public partial class TransformSetSiblingIndexResponse : MessageObject, IResponse
    {
        public static TransformSetSiblingIndexResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<TransformSetSiblingIndexResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string Name { get; set; }
        [MemoryPackOrder(4)]
        public string Path { get; set; }
        [MemoryPackOrder(5)]
        public int SiblingIndex { get; set; }
        [MemoryPackOrder(6)]
        public BridgeTransformInfo Transform { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.MenuItemExecuteRequest)]
    [ResponseType(nameof(MenuItemExecuteResponse))]
    public partial class MenuItemExecuteRequest : MessageObject, IRequest
    {
        public static MenuItemExecuteRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<MenuItemExecuteRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string MenuPath { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.MenuItemExecuteResponse)]
    public partial class MenuItemExecuteResponse : MessageObject, IResponse
    {
        public static MenuItemExecuteResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<MenuItemExecuteResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string MenuPath { get; set; }
        [MemoryPackOrder(4)]
        public bool Executed { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.PrefabInstantiateRequest)]
    [ResponseType(nameof(PrefabInstantiateResponse))]
    public partial class PrefabInstantiateRequest : MessageObject, IRequest
    {
        public static PrefabInstantiateRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabInstantiateRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string PrefabPath { get; set; }
        [MemoryPackOrder(2)]
        public BridgeVector3 Position { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.PrefabInstantiateResponse)]
    public partial class PrefabInstantiateResponse : MessageObject, IResponse
    {
        public static PrefabInstantiateResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabInstantiateResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string PrefabPath { get; set; }
        [MemoryPackOrder(4)]
        public BridgeObjectInfo Instance { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.PrefabSaveRequest)]
    [ResponseType(nameof(PrefabSaveResponse))]
    public partial class PrefabSaveRequest : MessageObject, IRequest
    {
        public static PrefabSaveRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabSaveRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string GameObjectPath { get; set; }
        [MemoryPackOrder(2)]
        public string SavePath { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.PrefabSaveResponse)]
    public partial class PrefabSaveResponse : MessageObject, IResponse
    {
        public static PrefabSaveResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabSaveResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string GameObjectName { get; set; }
        [MemoryPackOrder(4)]
        public string PrefabPath { get; set; }
        [MemoryPackOrder(5)]
        public bool Saved { get; set; }
        [MemoryPackOrder(6)]
        public BridgeAssetInfo Asset { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.PrefabUnpackRequest)]
    [ResponseType(nameof(PrefabUnpackResponse))]
    public partial class PrefabUnpackRequest : MessageObject, IRequest
    {
        public static PrefabUnpackRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabUnpackRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string GameObjectPath { get; set; }
        [MemoryPackOrder(2)]
        public bool Completely { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.PrefabUnpackResponse)]
    public partial class PrefabUnpackResponse : MessageObject, IResponse
    {
        public static PrefabUnpackResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabUnpackResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string GameObjectName { get; set; }
        [MemoryPackOrder(4)]
        public bool Unpacked { get; set; }
        [MemoryPackOrder(5)]
        public bool Completely { get; set; }
        [MemoryPackOrder(6)]
        public BridgeObjectInfo Object { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.PrefabGetInfoRequest)]
    [ResponseType(nameof(PrefabGetInfoResponse))]
    public partial class PrefabGetInfoRequest : MessageObject, IRequest
    {
        public static PrefabGetInfoRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabGetInfoRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string PrefabPath { get; set; }
        [MemoryPackOrder(2)]
        public string GameObjectPath { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.PrefabGetInfoResponse)]
    public partial class PrefabGetInfoResponse : MessageObject, IResponse
    {
        public static PrefabGetInfoResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabGetInfoResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string Name { get; set; }
        [MemoryPackOrder(4)]
        public bool IsPrefabAsset { get; set; }
        [MemoryPackOrder(5)]
        public bool IsPrefabInstance { get; set; }
        [MemoryPackOrder(6)]
        public string PrefabAssetPath { get; set; }
        [MemoryPackOrder(7)]
        public string PrefabType { get; set; }
        [MemoryPackOrder(8)]
        public string PrefabStatus { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.PrefabGetHierarchyRequest)]
    [ResponseType(nameof(PrefabGetHierarchyResponse))]
    public partial class PrefabGetHierarchyRequest : MessageObject, IRequest
    {
        public static PrefabGetHierarchyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabGetHierarchyRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string PrefabPath { get; set; }
        [MemoryPackOrder(2)]
        public int Depth { get; set; }
        [MemoryPackOrder(3)]
        public bool IncludeInactive { get; set; }
        [MemoryPackOrder(4)]
        public bool IncludeComponents { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.PrefabGetHierarchyResponse)]
    public partial class PrefabGetHierarchyResponse : MessageObject, IResponse
    {
        public static PrefabGetHierarchyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabGetHierarchyResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string PrefabPath { get; set; }
        [MemoryPackOrder(4)]
        public string PrefabName { get; set; }
        [MemoryPackOrder(5)]
        public int RootCount { get; set; }
        [MemoryPackOrder(6)]
        public bool Truncated { get; set; }
        [MemoryPackOrder(7)]
        public List<BridgeSceneNode> Roots { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.PrefabApplyRequest)]
    [ResponseType(nameof(PrefabApplyResponse))]
    public partial class PrefabApplyRequest : MessageObject, IRequest
    {
        public static PrefabApplyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabApplyRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string GameObjectPath { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.PrefabApplyResponse)]
    public partial class PrefabApplyResponse : MessageObject, IResponse
    {
        public static PrefabApplyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PrefabApplyResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string GameObjectName { get; set; }
        [MemoryPackOrder(4)]
        public string PrefabPath { get; set; }
        [MemoryPackOrder(5)]
        public bool Applied { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorGetComponentsRequest)]
    [ResponseType(nameof(InspectorGetComponentsResponse))]
    public partial class InspectorGetComponentsRequest : MessageObject, IRequest
    {
        public static InspectorGetComponentsRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetComponentsRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(4)]
        public string ObjectPath { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorGetComponentsResponse)]
    public partial class InspectorGetComponentsResponse : MessageObject, IResponse
    {
        public static InspectorGetComponentsResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetComponentsResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string GameObjectName { get; set; }
        [MemoryPackOrder(4)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(5)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(6)]
        public List<BridgeComponentInfo> Components { get; set; } = new();

        [MemoryPackOrder(7)]
        public int Count { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorGetPropertiesRequest)]
    [ResponseType(nameof(InspectorGetPropertiesResponse))]
    public partial class InspectorGetPropertiesRequest : MessageObject, IRequest
    {
        public static InspectorGetPropertiesRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetPropertiesRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(4)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(5)]
        public string ComponentName { get; set; }
        [MemoryPackOrder(6)]
        public int ComponentIndex { get; set; }
        [MemoryPackOrder(7)]
        public int ComponentInstanceId { get; set; }
        [MemoryPackOrder(8)]
        public bool IncludeChildren { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorGetPropertiesResponse)]
    public partial class InspectorGetPropertiesResponse : MessageObject, IResponse
    {
        public static InspectorGetPropertiesResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetPropertiesResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string TargetName { get; set; }
        [MemoryPackOrder(4)]
        public string TargetType { get; set; }
        [MemoryPackOrder(5)]
        public string GameObjectName { get; set; }
        [MemoryPackOrder(6)]
        public string ComponentName { get; set; }
        [MemoryPackOrder(7)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(8)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(9)]
        public List<BridgePropertyInfo> Properties { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorGetPropertyRequest)]
    [ResponseType(nameof(InspectorGetPropertyResponse))]
    public partial class InspectorGetPropertyRequest : MessageObject, IRequest
    {
        public static InspectorGetPropertyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetPropertyRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(4)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(5)]
        public string ComponentName { get; set; }
        [MemoryPackOrder(6)]
        public int ComponentIndex { get; set; }
        [MemoryPackOrder(7)]
        public int ComponentInstanceId { get; set; }
        [MemoryPackOrder(8)]
        public string PropertyName { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorGetPropertyResponse)]
    public partial class InspectorGetPropertyResponse : MessageObject, IResponse
    {
        public static InspectorGetPropertyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorGetPropertyResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string TargetName { get; set; }
        [MemoryPackOrder(4)]
        public string TargetType { get; set; }
        [MemoryPackOrder(5)]
        public string ComponentName { get; set; }
        [MemoryPackOrder(6)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(7)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(8)]
        public BridgePropertyInfo Property { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorFindPropertyRequest)]
    [ResponseType(nameof(InspectorFindPropertyResponse))]
    public partial class InspectorFindPropertyRequest : MessageObject, IRequest
    {
        public static InspectorFindPropertyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorFindPropertyRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(4)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(5)]
        public string ComponentName { get; set; }
        [MemoryPackOrder(6)]
        public int ComponentIndex { get; set; }
        [MemoryPackOrder(7)]
        public int ComponentInstanceId { get; set; }
        [MemoryPackOrder(8)]
        public string Keyword { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorFindPropertyResponse)]
    public partial class InspectorFindPropertyResponse : MessageObject, IResponse
    {
        public static InspectorFindPropertyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorFindPropertyResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string TargetName { get; set; }
        [MemoryPackOrder(4)]
        public string TargetType { get; set; }
        [MemoryPackOrder(5)]
        public string ComponentName { get; set; }
        [MemoryPackOrder(6)]
        public string Keyword { get; set; }
        [MemoryPackOrder(7)]
        public int Count { get; set; }
        [MemoryPackOrder(8)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(9)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(10)]
        public List<BridgePropertyInfo> Properties { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorSetPropertyRequest)]
    [ResponseType(nameof(InspectorSetPropertyResponse))]
    public partial class InspectorSetPropertyRequest : MessageObject, IRequest
    {
        public static InspectorSetPropertyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorSetPropertyRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(4)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(5)]
        public string ComponentName { get; set; }
        [MemoryPackOrder(6)]
        public int ComponentIndex { get; set; }
        [MemoryPackOrder(7)]
        public int ComponentInstanceId { get; set; }
        [MemoryPackOrder(8)]
        public string PropertyName { get; set; }
        [MemoryPackOrder(9)]
        public BridgePropertyInfo Value { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorSetPropertyResponse)]
    public partial class InspectorSetPropertyResponse : MessageObject, IResponse
    {
        public static InspectorSetPropertyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorSetPropertyResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string TargetName { get; set; }
        [MemoryPackOrder(4)]
        public string TargetType { get; set; }
        [MemoryPackOrder(5)]
        public string GameObjectName { get; set; }
        [MemoryPackOrder(6)]
        public string ComponentName { get; set; }
        [MemoryPackOrder(7)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(8)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(9)]
        public bool Changed { get; set; }
        [MemoryPackOrder(10)]
        public List<BridgePropertyInfo> Properties { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorSetPropertiesRequest)]
    [ResponseType(nameof(InspectorSetPropertiesResponse))]
    public partial class InspectorSetPropertiesRequest : MessageObject, IRequest
    {
        public static InspectorSetPropertiesRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorSetPropertiesRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(4)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(5)]
        public string ComponentName { get; set; }
        [MemoryPackOrder(6)]
        public int ComponentIndex { get; set; }
        [MemoryPackOrder(7)]
        public int ComponentInstanceId { get; set; }
        [MemoryPackOrder(8)]
        public List<BridgePropertyInfo> Values { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorSetPropertiesResponse)]
    public partial class InspectorSetPropertiesResponse : MessageObject, IResponse
    {
        public static InspectorSetPropertiesResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorSetPropertiesResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string TargetName { get; set; }
        [MemoryPackOrder(4)]
        public string TargetType { get; set; }
        [MemoryPackOrder(5)]
        public string GameObjectName { get; set; }
        [MemoryPackOrder(6)]
        public string ComponentName { get; set; }
        [MemoryPackOrder(7)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(8)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(9)]
        public bool Changed { get; set; }
        [MemoryPackOrder(10)]
        public List<BridgePropertyInfo> Properties { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorAddComponentRequest)]
    [ResponseType(nameof(InspectorAddComponentResponse))]
    public partial class InspectorAddComponentRequest : MessageObject, IRequest
    {
        public static InspectorAddComponentRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorAddComponentRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(4)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(5)]
        public string TypeName { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorAddComponentResponse)]
    public partial class InspectorAddComponentResponse : MessageObject, IResponse
    {
        public static InspectorAddComponentResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorAddComponentResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string GameObjectName { get; set; }
        [MemoryPackOrder(4)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(5)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(6)]
        public BridgeComponentInfo AddedComponent { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorRemoveComponentRequest)]
    [ResponseType(nameof(InspectorRemoveComponentResponse))]
    public partial class InspectorRemoveComponentRequest : MessageObject, IRequest
    {
        public static InspectorRemoveComponentRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorRemoveComponentRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(4)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(5)]
        public string ComponentName { get; set; }
        [MemoryPackOrder(6)]
        public int ComponentIndex { get; set; }
        [MemoryPackOrder(7)]
        public int ComponentInstanceId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.InspectorRemoveComponentResponse)]
    public partial class InspectorRemoveComponentResponse : MessageObject, IResponse
    {
        public static InspectorRemoveComponentResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<InspectorRemoveComponentResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string GameObjectName { get; set; }
        [MemoryPackOrder(4)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(5)]
        public string ObjectPath { get; set; }
        [MemoryPackOrder(6)]
        public BridgeComponentInfo RemovedComponent { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.EditorUndoRequest)]
    [ResponseType(nameof(EditorUndoResponse))]
    public partial class EditorUndoRequest : MessageObject, IRequest
    {
        public static EditorUndoRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorUndoRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Count { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.EditorUndoResponse)]
    public partial class EditorUndoResponse : MessageObject, IResponse
    {
        public static EditorUndoResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorUndoResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public int Count { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.EditorRedoRequest)]
    [ResponseType(nameof(EditorRedoResponse))]
    public partial class EditorRedoRequest : MessageObject, IRequest
    {
        public static EditorRedoRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorRedoRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Count { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.EditorRedoResponse)]
    public partial class EditorRedoResponse : MessageObject, IResponse
    {
        public static EditorRedoResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorRedoResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public int Count { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.EditorPauseRequest)]
    [ResponseType(nameof(EditorPauseResponse))]
    public partial class EditorPauseRequest : MessageObject, IRequest
    {
        public static EditorPauseRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorPauseRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public bool Toggle { get; set; }
        [MemoryPackOrder(2)]
        public bool Pause { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.EditorPauseResponse)]
    public partial class EditorPauseResponse : MessageObject, IResponse
    {
        public static EditorPauseResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorPauseResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public bool IsPaused { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.EditorGetStateRequest)]
    [ResponseType(nameof(EditorGetStateResponse))]
    public partial class EditorGetStateRequest : MessageObject, IRequest
    {
        public static EditorGetStateRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorGetStateRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.EditorGetStateResponse)]
    public partial class EditorGetStateResponse : MessageObject, IResponse
    {
        public static EditorGetStateResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorGetStateResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public bool IsPlaying { get; set; }
        [MemoryPackOrder(4)]
        public bool IsPaused { get; set; }
        [MemoryPackOrder(5)]
        public bool IsCompiling { get; set; }
        [MemoryPackOrder(6)]
        public bool IsUpdating { get; set; }
        [MemoryPackOrder(7)]
        public string ApplicationPath { get; set; }
        [MemoryPackOrder(8)]
        public string ApplicationContentsPath { get; set; }
        [MemoryPackOrder(9)]
        public bool EnterPlayModeOptionsEnabled { get; set; }
        [MemoryPackOrder(10)]
        public string EnterPlayModeOptions { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BatchExecuteRequest)]
    [ResponseType(nameof(BatchExecuteResponse))]
    public partial class BatchExecuteRequest : MessageObject, IRequest
    {
        public static BatchExecuteRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BatchExecuteRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public List<string> Commands { get; set; } = new();

        [MemoryPackOrder(2)]
        public bool StopOnError { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BatchExecuteResponse)]
    public partial class BatchExecuteResponse : MessageObject, IResponse
    {
        public static BatchExecuteResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BatchExecuteResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public List<BridgeBatchStepResult> Results { get; set; } = new();

        [MemoryPackOrder(4)]
        public int Count { get; set; }
        [MemoryPackOrder(5)]
        public int Failed { get; set; }
        [MemoryPackOrder(6)]
        public bool Completed { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort GameObjectCreateRequest = 11401;
        public const ushort GameObjectCreateResponse = 11402;
        public const ushort GameObjectDestroyRequest = 11403;
        public const ushort GameObjectDestroyResponse = 11404;
        public const ushort GameObjectFindRequest = 11405;
        public const ushort GameObjectFindResponse = 11406;
        public const ushort GameObjectSetActiveRequest = 11407;
        public const ushort GameObjectSetActiveResponse = 11408;
        public const ushort GameObjectRenameRequest = 11409;
        public const ushort GameObjectRenameResponse = 11410;
        public const ushort GameObjectDuplicateRequest = 11411;
        public const ushort GameObjectDuplicateResponse = 11412;
        public const ushort GameObjectGetInfoRequest = 11413;
        public const ushort GameObjectGetInfoResponse = 11414;
        public const ushort TransformGetRequest = 11415;
        public const ushort TransformGetResponse = 11416;
        public const ushort TransformSetPositionRequest = 11417;
        public const ushort TransformSetPositionResponse = 11418;
        public const ushort TransformSetRotationRequest = 11419;
        public const ushort TransformSetRotationResponse = 11420;
        public const ushort TransformSetScaleRequest = 11421;
        public const ushort TransformSetScaleResponse = 11422;
        public const ushort TransformSetParentRequest = 11423;
        public const ushort TransformSetParentResponse = 11424;
        public const ushort TransformLookAtRequest = 11425;
        public const ushort TransformLookAtResponse = 11426;
        public const ushort TransformResetRequest = 11427;
        public const ushort TransformResetResponse = 11428;
        public const ushort TransformSetSiblingIndexRequest = 11429;
        public const ushort TransformSetSiblingIndexResponse = 11430;
        public const ushort MenuItemExecuteRequest = 11431;
        public const ushort MenuItemExecuteResponse = 11432;
        public const ushort PrefabInstantiateRequest = 11433;
        public const ushort PrefabInstantiateResponse = 11434;
        public const ushort PrefabSaveRequest = 11435;
        public const ushort PrefabSaveResponse = 11436;
        public const ushort PrefabUnpackRequest = 11437;
        public const ushort PrefabUnpackResponse = 11438;
        public const ushort PrefabGetInfoRequest = 11439;
        public const ushort PrefabGetInfoResponse = 11440;
        public const ushort PrefabGetHierarchyRequest = 11441;
        public const ushort PrefabGetHierarchyResponse = 11442;
        public const ushort PrefabApplyRequest = 11443;
        public const ushort PrefabApplyResponse = 11444;
        public const ushort InspectorGetComponentsRequest = 11445;
        public const ushort InspectorGetComponentsResponse = 11446;
        public const ushort InspectorGetPropertiesRequest = 11447;
        public const ushort InspectorGetPropertiesResponse = 11448;
        public const ushort InspectorGetPropertyRequest = 11449;
        public const ushort InspectorGetPropertyResponse = 11450;
        public const ushort InspectorFindPropertyRequest = 11451;
        public const ushort InspectorFindPropertyResponse = 11452;
        public const ushort InspectorSetPropertyRequest = 11453;
        public const ushort InspectorSetPropertyResponse = 11454;
        public const ushort InspectorSetPropertiesRequest = 11455;
        public const ushort InspectorSetPropertiesResponse = 11456;
        public const ushort InspectorAddComponentRequest = 11457;
        public const ushort InspectorAddComponentResponse = 11458;
        public const ushort InspectorRemoveComponentRequest = 11459;
        public const ushort InspectorRemoveComponentResponse = 11460;
        public const ushort EditorUndoRequest = 11461;
        public const ushort EditorUndoResponse = 11462;
        public const ushort EditorRedoRequest = 11463;
        public const ushort EditorRedoResponse = 11464;
        public const ushort EditorPauseRequest = 11465;
        public const ushort EditorPauseResponse = 11466;
        public const ushort EditorGetStateRequest = 11467;
        public const ushort EditorGetStateResponse = 11468;
        public const ushort BatchExecuteRequest = 11469;
        public const ushort BatchExecuteResponse = 11470;
    }
}