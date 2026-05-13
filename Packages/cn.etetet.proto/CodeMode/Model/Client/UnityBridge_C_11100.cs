using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(Opcode.Ping)]
    [ResponseType(nameof(PingResponse))]
    public partial class Ping : MessageObject, IRequest
    {
        public static Ping Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Ping>(isFromPool);
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
    [Message(Opcode.PingResponse)]
    public partial class PingResponse : MessageObject, IResponse
    {
        public static PingResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PingResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public long Time { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Time = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.HostState)]
    [ResponseType(nameof(HostStateResponse))]
    public partial class HostState : MessageObject, IRequest
    {
        public static HostState Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<HostState>(isFromPool);
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
    [Message(Opcode.HostStateResponse)]
    public partial class HostStateResponse : MessageObject, IResponse
    {
        public static HostStateResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<HostStateResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public bool IsCompiling { get; set; }
        [MemoryPackOrder(4)]
        public bool IsPlaying { get; set; }
        [MemoryPackOrder(5)]
        public bool IsPlayingOrWillChangePlaymode { get; set; }
        [MemoryPackOrder(6)]
        public string CodeMode { get; set; }
        [MemoryPackOrder(7)]
        public string UnityVersion { get; set; }
        [MemoryPackOrder(8)]
        public string AvailableCommands { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.IsCompiling = default;
            this.IsPlaying = default;
            this.IsPlayingOrWillChangePlaymode = default;
            this.CodeMode = default;
            this.UnityVersion = default;
            this.AvailableCommands = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.Compile)]
    [ResponseType(nameof(CompileResponse))]
    public partial class Compile : MessageObject, IRequest
    {
        public static Compile Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Compile>(isFromPool);
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
    [Message(Opcode.CompileResponse)]
    public partial class CompileResponse : MessageObject, IResponse
    {
        public static CompileResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<CompileResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public long DurationMs { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.DurationMs = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.Reload)]
    [ResponseType(nameof(ReloadResponse))]
    public partial class Reload : MessageObject, IRequest
    {
        public static Reload Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Reload>(isFromPool);
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
    [Message(Opcode.ReloadResponse)]
    public partial class ReloadResponse : MessageObject, IResponse
    {
        public static ReloadResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ReloadResponse>(isFromPool);
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
    [Message(Opcode.EnterPlay)]
    [ResponseType(nameof(EnterPlayResponse))]
    public partial class EnterPlay : MessageObject, IRequest
    {
        public static EnterPlay Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EnterPlay>(isFromPool);
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
    [Message(Opcode.EnterPlayResponse)]
    public partial class EnterPlayResponse : MessageObject, IResponse
    {
        public static EnterPlayResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EnterPlayResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public bool IsPlaying { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.IsPlaying = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.ExitPlay)]
    [ResponseType(nameof(ExitPlayResponse))]
    public partial class ExitPlay : MessageObject, IRequest
    {
        public static ExitPlay Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ExitPlay>(isFromPool);
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
    [Message(Opcode.ExitPlayResponse)]
    public partial class ExitPlayResponse : MessageObject, IResponse
    {
        public static ExitPlayResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ExitPlayResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public bool IsPlaying { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.IsPlaying = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.RegenProject)]
    [ResponseType(nameof(RegenProjectResponse))]
    public partial class RegenProject : MessageObject, IRequest
    {
        public static RegenProject Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RegenProject>(isFromPool);
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
    [Message(Opcode.RegenProjectResponse)]
    public partial class RegenProjectResponse : MessageObject, IResponse
    {
        public static RegenProjectResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RegenProjectResponse>(isFromPool);
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
    [Message(Opcode.ErrorResponse)]
    public partial class ErrorResponse : MessageObject, IResponse
    {
        public static ErrorResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ErrorResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string Command { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Command = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.Refresh)]
    [ResponseType(nameof(RefreshResponse))]
    public partial class Refresh : MessageObject, IRequest
    {
        public static Refresh Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Refresh>(isFromPool);
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
    [Message(Opcode.RefreshResponse)]
    public partial class RefreshResponse : MessageObject, IResponse
    {
        public static RefreshResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RefreshResponse>(isFromPool);
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

    [MemoryPackable]
    [Message(Opcode.BridgeVector2)]
    public partial class BridgeVector2 : MessageObject
    {
        public static BridgeVector2 Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeVector2>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public float X { get; set; }
        [MemoryPackOrder(1)]
        public float Y { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.X = default;
            this.Y = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BridgeVector3)]
    public partial class BridgeVector3 : MessageObject
    {
        public static BridgeVector3 Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeVector3>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public float X { get; set; }
        [MemoryPackOrder(1)]
        public float Y { get; set; }
        [MemoryPackOrder(2)]
        public float Z { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.X = default;
            this.Y = default;
            this.Z = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BridgeQuaternion)]
    public partial class BridgeQuaternion : MessageObject
    {
        public static BridgeQuaternion Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeQuaternion>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public float X { get; set; }
        [MemoryPackOrder(1)]
        public float Y { get; set; }
        [MemoryPackOrder(2)]
        public float Z { get; set; }
        [MemoryPackOrder(3)]
        public float W { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.X = default;
            this.Y = default;
            this.Z = default;
            this.W = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BridgeTransformInfo)]
    public partial class BridgeTransformInfo : MessageObject
    {
        public static BridgeTransformInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeTransformInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public BridgeVector3 LocalPosition { get; set; }
        [MemoryPackOrder(1)]
        public BridgeVector3 LocalEulerAngles { get; set; }
        [MemoryPackOrder(2)]
        public BridgeQuaternion LocalRotation { get; set; }
        [MemoryPackOrder(3)]
        public BridgeVector3 LocalScale { get; set; }
        [MemoryPackOrder(4)]
        public string ParentPath { get; set; }
        [MemoryPackOrder(5)]
        public int SiblingIndex { get; set; }
        [MemoryPackOrder(6)]
        public BridgeVector3 Position { get; set; }
        [MemoryPackOrder(7)]
        public BridgeVector3 EulerAngles { get; set; }
        [MemoryPackOrder(8)]
        public int ChildCount { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.LocalPosition = default;
            this.LocalEulerAngles = default;
            this.LocalRotation = default;
            this.LocalScale = default;
            this.ParentPath = default;
            this.SiblingIndex = default;
            this.Position = default;
            this.EulerAngles = default;
            this.ChildCount = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BridgeObjectInfo)]
    public partial class BridgeObjectInfo : MessageObject
    {
        public static BridgeObjectInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeObjectInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string Name { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(3)]
        public bool ActiveSelf { get; set; }
        [MemoryPackOrder(4)]
        public bool ActiveInHierarchy { get; set; }
        [MemoryPackOrder(5)]
        public string Tag { get; set; }
        [MemoryPackOrder(6)]
        public string LayerName { get; set; }
        [MemoryPackOrder(7)]
        public int Layer { get; set; }
        [MemoryPackOrder(8)]
        public BridgeTransformInfo Transform { get; set; }
        [MemoryPackOrder(9)]
        public List<BridgeComponentInfo> Components { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Name = default;
            this.Path = default;
            this.InstanceId = default;
            this.ActiveSelf = default;
            this.ActiveInHierarchy = default;
            this.Tag = default;
            this.LayerName = default;
            this.Layer = default;
            this.Transform = default;
            this.Components.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BridgeAssetInfo)]
    public partial class BridgeAssetInfo : MessageObject
    {
        public static BridgeAssetInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeAssetInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(1)]
        public string Guid { get; set; }
        [MemoryPackOrder(2)]
        public string TypeName { get; set; }
        [MemoryPackOrder(3)]
        public string Name { get; set; }
        [MemoryPackOrder(4)]
        public string Extension { get; set; }
        [MemoryPackOrder(5)]
        public long FileSize { get; set; }
        [MemoryPackOrder(6)]
        public int InstanceId { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.AssetPath = default;
            this.Guid = default;
            this.TypeName = default;
            this.Name = default;
            this.Extension = default;
            this.FileSize = default;
            this.InstanceId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BridgeSceneNode)]
    public partial class BridgeSceneNode : MessageObject
    {
        public static BridgeSceneNode Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeSceneNode>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public BridgeObjectInfo Object { get; set; }
        [MemoryPackOrder(1)]
        public List<BridgeSceneNode> Children { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Object = default;
            this.Children.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BridgeComponentInfo)]
    public partial class BridgeComponentInfo : MessageObject
    {
        public static BridgeComponentInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeComponentInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string TypeName { get; set; }
        [MemoryPackOrder(1)]
        public string FullTypeName { get; set; }
        [MemoryPackOrder(2)]
        public int ComponentIndex { get; set; }
        [MemoryPackOrder(3)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(4)]
        public bool Enabled { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.TypeName = default;
            this.FullTypeName = default;
            this.ComponentIndex = default;
            this.InstanceId = default;
            this.Enabled = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BridgePropertyInfo)]
    public partial class BridgePropertyInfo : MessageObject
    {
        public static BridgePropertyInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgePropertyInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string Name { get; set; }
        [MemoryPackOrder(1)]
        public string DisplayName { get; set; }
        [MemoryPackOrder(2)]
        public string Type { get; set; }
        [MemoryPackOrder(3)]
        public string StringValue { get; set; }
        [MemoryPackOrder(4)]
        public int IntValue { get; set; }
        [MemoryPackOrder(5)]
        public float FloatValue { get; set; }
        [MemoryPackOrder(6)]
        public bool BoolValue { get; set; }
        [MemoryPackOrder(7)]
        public BridgeVector2 Vector2Value { get; set; }
        [MemoryPackOrder(8)]
        public BridgeVector3 Vector3Value { get; set; }
        [MemoryPackOrder(9)]
        public string ObjectReferencePath { get; set; }
        [MemoryPackOrder(10)]
        public string ObjectReferenceType { get; set; }
        [MemoryPackOrder(11)]
        public bool IsArray { get; set; }
        [MemoryPackOrder(12)]
        public bool IsEditable { get; set; }
        [MemoryPackOrder(13)]
        public string PropertyPath { get; set; }
        [MemoryPackOrder(14)]
        public bool IsExpanded { get; set; }
        [MemoryPackOrder(15)]
        public bool HasChildren { get; set; }
        [MemoryPackOrder(16)]
        public int Depth { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Name = default;
            this.DisplayName = default;
            this.Type = default;
            this.StringValue = default;
            this.IntValue = default;
            this.FloatValue = default;
            this.BoolValue = default;
            this.Vector2Value = default;
            this.Vector3Value = default;
            this.ObjectReferencePath = default;
            this.ObjectReferenceType = default;
            this.IsArray = default;
            this.IsEditable = default;
            this.PropertyPath = default;
            this.IsExpanded = default;
            this.HasChildren = default;
            this.Depth = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BridgeConsoleLog)]
    public partial class BridgeConsoleLog : MessageObject
    {
        public static BridgeConsoleLog Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeConsoleLog>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string LogType { get; set; }
        [MemoryPackOrder(1)]
        public string Message { get; set; }
        [MemoryPackOrder(2)]
        public string StackTrace { get; set; }
        [MemoryPackOrder(3)]
        public string Time { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.LogType = default;
            this.Message = default;
            this.StackTrace = default;
            this.Time = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BridgeGameViewResolution)]
    public partial class BridgeGameViewResolution : MessageObject
    {
        public static BridgeGameViewResolution Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeGameViewResolution>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int Width { get; set; }
        [MemoryPackOrder(1)]
        public int Height { get; set; }
        [MemoryPackOrder(2)]
        public string Label { get; set; }
        [MemoryPackOrder(3)]
        public bool IsCurrent { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Width = default;
            this.Height = default;
            this.Label = default;
            this.IsCurrent = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BridgeScreenshotInfo)]
    public partial class BridgeScreenshotInfo : MessageObject
    {
        public static BridgeScreenshotInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeScreenshotInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string Path { get; set; }
        [MemoryPackOrder(1)]
        public string FileName { get; set; }
        [MemoryPackOrder(2)]
        public int Width { get; set; }
        [MemoryPackOrder(3)]
        public int Height { get; set; }
        [MemoryPackOrder(4)]
        public long FileSize { get; set; }
        [MemoryPackOrder(5)]
        public string MediaType { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Path = default;
            this.FileName = default;
            this.Width = default;
            this.Height = default;
            this.FileSize = default;
            this.MediaType = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BridgeBatchStepResult)]
    public partial class BridgeBatchStepResult : MessageObject
    {
        public static BridgeBatchStepResult Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeBatchStepResult>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string Name { get; set; }
        [MemoryPackOrder(1)]
        public string Command { get; set; }
        [MemoryPackOrder(2)]
        public int Error { get; set; }
        [MemoryPackOrder(3)]
        public string Message { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Name = default;
            this.Command = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.BridgeTestResult)]
    public partial class BridgeTestResult : MessageObject
    {
        public static BridgeTestResult Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<BridgeTestResult>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public string Name { get; set; }
        [MemoryPackOrder(1)]
        public string FullName { get; set; }
        [MemoryPackOrder(2)]
        public bool Passed { get; set; }
        [MemoryPackOrder(3)]
        public int Error { get; set; }
        [MemoryPackOrder(4)]
        public string Message { get; set; }
        [MemoryPackOrder(5)]
        public long DurationMs { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Name = default;
            this.FullName = default;
            this.Passed = default;
            this.Error = default;
            this.Message = default;
            this.DurationMs = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetSearchRequest)]
    [ResponseType(nameof(AssetSearchResponse))]
    public partial class AssetSearchRequest : MessageObject, IRequest
    {
        public static AssetSearchRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetSearchRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Mode { get; set; }
        [MemoryPackOrder(2)]
        public string Filter { get; set; }
        [MemoryPackOrder(3)]
        public string Keyword { get; set; }
        [MemoryPackOrder(4)]
        public List<string> SearchInFolders { get; set; } = new();

        [MemoryPackOrder(5)]
        public int MaxResults { get; set; }
        [MemoryPackOrder(6)]
        public string Format { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Mode = default;
            this.Filter = default;
            this.Keyword = default;
            this.SearchInFolders.Clear();
            this.MaxResults = default;
            this.Format = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetSearchResponse)]
    public partial class AssetSearchResponse : MessageObject, IResponse
    {
        public static AssetSearchResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetSearchResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public List<BridgeAssetInfo> Assets { get; set; } = new();

        [MemoryPackOrder(4)]
        public List<string> Paths { get; set; } = new();

        [MemoryPackOrder(5)]
        public string Mode { get; set; }
        [MemoryPackOrder(6)]
        public string Filter { get; set; }
        [MemoryPackOrder(7)]
        public int TotalFound { get; set; }
        [MemoryPackOrder(8)]
        public int Returned { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Assets.Clear();
            this.Paths.Clear();
            this.Mode = default;
            this.Filter = default;
            this.TotalFound = default;
            this.Returned = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetFindRequest)]
    [ResponseType(nameof(AssetFindResponse))]
    public partial class AssetFindRequest : MessageObject, IRequest
    {
        public static AssetFindRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetFindRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Filter { get; set; }
        [MemoryPackOrder(2)]
        public List<string> SearchInFolders { get; set; } = new();

        [MemoryPackOrder(3)]
        public int MaxResults { get; set; }
        [MemoryPackOrder(4)]
        public string Format { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Filter = default;
            this.SearchInFolders.Clear();
            this.MaxResults = default;
            this.Format = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetFindResponse)]
    public partial class AssetFindResponse : MessageObject, IResponse
    {
        public static AssetFindResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetFindResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public List<BridgeAssetInfo> Assets { get; set; } = new();

        [MemoryPackOrder(4)]
        public List<string> Paths { get; set; } = new();

        [MemoryPackOrder(5)]
        public string Filter { get; set; }
        [MemoryPackOrder(6)]
        public int TotalFound { get; set; }
        [MemoryPackOrder(7)]
        public int Returned { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Assets.Clear();
            this.Paths.Clear();
            this.Filter = default;
            this.TotalFound = default;
            this.Returned = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetGetPathRequest)]
    [ResponseType(nameof(AssetGetPathResponse))]
    public partial class AssetGetPathRequest : MessageObject, IRequest
    {
        public static AssetGetPathRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetGetPathRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Guid { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Guid = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetGetPathResponse)]
    public partial class AssetGetPathResponse : MessageObject, IResponse
    {
        public static AssetGetPathResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetGetPathResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string Guid { get; set; }
        [MemoryPackOrder(4)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(5)]
        public bool Exists { get; set; }
        [MemoryPackOrder(6)]
        public BridgeAssetInfo Asset { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Guid = default;
            this.AssetPath = default;
            this.Exists = default;
            this.Asset = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetLoadRequest)]
    [ResponseType(nameof(AssetLoadResponse))]
    public partial class AssetLoadRequest : MessageObject, IRequest
    {
        public static AssetLoadRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetLoadRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string AssetPath { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.AssetPath = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetLoadResponse)]
    public partial class AssetLoadResponse : MessageObject, IResponse
    {
        public static AssetLoadResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetLoadResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public BridgeAssetInfo Asset { get; set; }
        [MemoryPackOrder(4)]
        public bool Exists { get; set; }
        [MemoryPackOrder(5)]
        public int InstanceId { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Asset = default;
            this.Exists = default;
            this.InstanceId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetImportRequest)]
    [ResponseType(nameof(AssetImportResponse))]
    public partial class AssetImportRequest : MessageObject, IRequest
    {
        public static AssetImportRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetImportRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(2)]
        public bool ForceUpdate { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.AssetPath = default;
            this.ForceUpdate = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetImportResponse)]
    public partial class AssetImportResponse : MessageObject, IResponse
    {
        public static AssetImportResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetImportResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(4)]
        public bool Imported { get; set; }
        [MemoryPackOrder(5)]
        public BridgeAssetInfo Asset { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.AssetPath = default;
            this.Imported = default;
            this.Asset = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetRefreshRequest)]
    [ResponseType(nameof(AssetRefreshResponse))]
    public partial class AssetRefreshRequest : MessageObject, IRequest
    {
        public static AssetRefreshRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetRefreshRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public bool ForceUpdate { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.ForceUpdate = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetRefreshResponse)]
    public partial class AssetRefreshResponse : MessageObject, IResponse
    {
        public static AssetRefreshResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetRefreshResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public bool Refreshed { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Refreshed = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetReadTextRequest)]
    [ResponseType(nameof(AssetReadTextResponse))]
    public partial class AssetReadTextRequest : MessageObject, IRequest
    {
        public static AssetReadTextRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetReadTextRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(2)]
        public int StartLine { get; set; }
        [MemoryPackOrder(3)]
        public int MaxLines { get; set; }
        [MemoryPackOrder(4)]
        public int MaxChars { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.AssetPath = default;
            this.StartLine = default;
            this.MaxLines = default;
            this.MaxChars = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.AssetReadTextResponse)]
    public partial class AssetReadTextResponse : MessageObject, IResponse
    {
        public static AssetReadTextResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AssetReadTextResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(4)]
        public int TotalLines { get; set; }
        [MemoryPackOrder(5)]
        public int ReturnedLineStart { get; set; }
        [MemoryPackOrder(6)]
        public int ReturnedLineEnd { get; set; }
        [MemoryPackOrder(7)]
        public int ReturnedLineCount { get; set; }
        [MemoryPackOrder(8)]
        public bool Truncated { get; set; }
        [MemoryPackOrder(9)]
        public string Content { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.AssetPath = default;
            this.TotalLines = default;
            this.ReturnedLineStart = default;
            this.ReturnedLineEnd = default;
            this.ReturnedLineCount = default;
            this.Truncated = default;
            this.Content = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SceneGetHierarchyRequest)]
    [ResponseType(nameof(SceneGetHierarchyResponse))]
    public partial class SceneGetHierarchyRequest : MessageObject, IRequest
    {
        public static SceneGetHierarchyRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneGetHierarchyRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Depth { get; set; }
        [MemoryPackOrder(2)]
        public bool IncludeInactive { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Depth = default;
            this.IncludeInactive = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SceneGetHierarchyResponse)]
    public partial class SceneGetHierarchyResponse : MessageObject, IResponse
    {
        public static SceneGetHierarchyResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneGetHierarchyResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string SceneName { get; set; }
        [MemoryPackOrder(4)]
        public string ScenePath { get; set; }
        [MemoryPackOrder(5)]
        public int RootCount { get; set; }
        [MemoryPackOrder(6)]
        public List<BridgeSceneNode> Roots { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.SceneName = default;
            this.ScenePath = default;
            this.RootCount = default;
            this.Roots.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SceneGetActiveRequest)]
    [ResponseType(nameof(SceneGetActiveResponse))]
    public partial class SceneGetActiveRequest : MessageObject, IRequest
    {
        public static SceneGetActiveRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneGetActiveRequest>(isFromPool);
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
    [Message(Opcode.SceneGetActiveResponse)]
    public partial class SceneGetActiveResponse : MessageObject, IResponse
    {
        public static SceneGetActiveResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneGetActiveResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string SceneName { get; set; }
        [MemoryPackOrder(4)]
        public string ScenePath { get; set; }
        [MemoryPackOrder(5)]
        public bool IsLoaded { get; set; }
        [MemoryPackOrder(6)]
        public bool IsDirty { get; set; }
        [MemoryPackOrder(7)]
        public int RootCount { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.SceneName = default;
            this.ScenePath = default;
            this.IsLoaded = default;
            this.IsDirty = default;
            this.RootCount = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SceneLoadRequest)]
    [ResponseType(nameof(SceneLoadResponse))]
    public partial class SceneLoadRequest : MessageObject, IRequest
    {
        public static SceneLoadRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneLoadRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string ScenePath { get; set; }
        [MemoryPackOrder(2)]
        public string Mode { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.ScenePath = default;
            this.Mode = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SceneLoadResponse)]
    public partial class SceneLoadResponse : MessageObject, IResponse
    {
        public static SceneLoadResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneLoadResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string SceneName { get; set; }
        [MemoryPackOrder(4)]
        public string ScenePath { get; set; }
        [MemoryPackOrder(5)]
        public bool Loaded { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.SceneName = default;
            this.ScenePath = default;
            this.Loaded = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SceneSaveRequest)]
    [ResponseType(nameof(SceneSaveResponse))]
    public partial class SceneSaveRequest : MessageObject, IRequest
    {
        public static SceneSaveRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneSaveRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string SaveAs { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.SaveAs = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SceneSaveResponse)]
    public partial class SceneSaveResponse : MessageObject, IResponse
    {
        public static SceneSaveResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneSaveResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string SceneName { get; set; }
        [MemoryPackOrder(4)]
        public string ScenePath { get; set; }
        [MemoryPackOrder(5)]
        public bool Saved { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.SceneName = default;
            this.ScenePath = default;
            this.Saved = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SceneNewRequest)]
    [ResponseType(nameof(SceneNewResponse))]
    public partial class SceneNewRequest : MessageObject, IRequest
    {
        public static SceneNewRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneNewRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Setup { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Setup = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SceneNewResponse)]
    public partial class SceneNewResponse : MessageObject, IResponse
    {
        public static SceneNewResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SceneNewResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public string SceneName { get; set; }
        [MemoryPackOrder(4)]
        public string ScenePath { get; set; }
        [MemoryPackOrder(5)]
        public bool Created { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.SceneName = default;
            this.ScenePath = default;
            this.Created = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SelectionGetRequest)]
    [ResponseType(nameof(SelectionGetResponse))]
    public partial class SelectionGetRequest : MessageObject, IRequest
    {
        public static SelectionGetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionGetRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public bool IncludeComponents { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.IncludeComponents = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SelectionGetResponse)]
    public partial class SelectionGetResponse : MessageObject, IResponse
    {
        public static SelectionGetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionGetResponse>(isFromPool);
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
        public List<BridgeAssetInfo> Assets { get; set; } = new();

        [MemoryPackOrder(5)]
        public string ActiveObjectName { get; set; }
        [MemoryPackOrder(6)]
        public int ActiveObjectInstanceId { get; set; }
        [MemoryPackOrder(7)]
        public int Count { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Objects.Clear();
            this.Assets.Clear();
            this.ActiveObjectName = default;
            this.ActiveObjectInstanceId = default;
            this.Count = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SelectionSetRequest)]
    [ResponseType(nameof(SelectionSetResponse))]
    public partial class SelectionSetRequest : MessageObject, IRequest
    {
        public static SelectionSetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionSetRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(3)]
        public int InstanceId { get; set; }
        [MemoryPackOrder(4)]
        public List<int> InstanceIds { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.AssetPath = default;
            this.InstanceId = default;
            this.InstanceIds.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SelectionSetResponse)]
    public partial class SelectionSetResponse : MessageObject, IResponse
    {
        public static SelectionSetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionSetResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public int SelectedCount { get; set; }
        [MemoryPackOrder(4)]
        public string ActiveObjectName { get; set; }
        [MemoryPackOrder(5)]
        public List<BridgeObjectInfo> Objects { get; set; } = new();

        [MemoryPackOrder(6)]
        public List<BridgeAssetInfo> Assets { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.SelectedCount = default;
            this.ActiveObjectName = default;
            this.Objects.Clear();
            this.Assets.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SelectionAddRequest)]
    [ResponseType(nameof(SelectionAddResponse))]
    public partial class SelectionAddRequest : MessageObject, IRequest
    {
        public static SelectionAddRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionAddRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(3)]
        public int InstanceId { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.AssetPath = default;
            this.InstanceId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SelectionAddResponse)]
    public partial class SelectionAddResponse : MessageObject, IResponse
    {
        public static SelectionAddResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionAddResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public bool Added { get; set; }
        [MemoryPackOrder(4)]
        public string ObjectName { get; set; }
        [MemoryPackOrder(5)]
        public int SelectedCount { get; set; }
        [MemoryPackOrder(6)]
        public List<BridgeObjectInfo> Objects { get; set; } = new();

        [MemoryPackOrder(7)]
        public List<BridgeAssetInfo> Assets { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Added = default;
            this.ObjectName = default;
            this.SelectedCount = default;
            this.Objects.Clear();
            this.Assets.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SelectionRemoveRequest)]
    [ResponseType(nameof(SelectionRemoveResponse))]
    public partial class SelectionRemoveRequest : MessageObject, IRequest
    {
        public static SelectionRemoveRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionRemoveRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Path { get; set; }
        [MemoryPackOrder(2)]
        public string AssetPath { get; set; }
        [MemoryPackOrder(3)]
        public int InstanceId { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Path = default;
            this.AssetPath = default;
            this.InstanceId = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SelectionRemoveResponse)]
    public partial class SelectionRemoveResponse : MessageObject, IResponse
    {
        public static SelectionRemoveResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionRemoveResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public bool Removed { get; set; }
        [MemoryPackOrder(4)]
        public string ObjectName { get; set; }
        [MemoryPackOrder(5)]
        public int SelectedCount { get; set; }
        [MemoryPackOrder(6)]
        public List<BridgeObjectInfo> Objects { get; set; } = new();

        [MemoryPackOrder(7)]
        public List<BridgeAssetInfo> Assets { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Removed = default;
            this.ObjectName = default;
            this.SelectedCount = default;
            this.Objects.Clear();
            this.Assets.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.SelectionClearRequest)]
    [ResponseType(nameof(SelectionClearResponse))]
    public partial class SelectionClearRequest : MessageObject, IRequest
    {
        public static SelectionClearRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionClearRequest>(isFromPool);
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
    [Message(Opcode.SelectionClearResponse)]
    public partial class SelectionClearResponse : MessageObject, IResponse
    {
        public static SelectionClearResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SelectionClearResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public bool Cleared { get; set; }
        [MemoryPackOrder(4)]
        public int SelectedCount { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Cleared = default;
            this.SelectedCount = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.UnityTestRunRequest)]
    [ResponseType(nameof(UnityTestRunResponse))]
    public partial class UnityTestRunRequest : MessageObject, IRequest
    {
        public static UnityTestRunRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<UnityTestRunRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Name { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Name = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.UnityTestRunResponse)]
    public partial class UnityTestRunResponse : MessageObject, IResponse
    {
        public static UnityTestRunResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<UnityTestRunResponse>(isFromPool);
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
        public int Matched { get; set; }
        [MemoryPackOrder(5)]
        public int Passed { get; set; }
        [MemoryPackOrder(6)]
        public int Failed { get; set; }
        [MemoryPackOrder(7)]
        public long DurationMs { get; set; }
        [MemoryPackOrder(8)]
        public List<BridgeTestResult> Results { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Name = default;
            this.Matched = default;
            this.Passed = default;
            this.Failed = default;
            this.DurationMs = default;
            this.Results.Clear();

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.ConsoleGetLogsRequest)]
    [ResponseType(nameof(ConsoleGetLogsResponse))]
    public partial class ConsoleGetLogsRequest : MessageObject, IRequest
    {
        public static ConsoleGetLogsRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ConsoleGetLogsRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Count { get; set; }
        [MemoryPackOrder(2)]
        public string LogType { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Count = default;
            this.LogType = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.ConsoleGetLogsResponse)]
    public partial class ConsoleGetLogsResponse : MessageObject, IResponse
    {
        public static ConsoleGetLogsResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ConsoleGetLogsResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public List<BridgeConsoleLog> Logs { get; set; } = new();

        [MemoryPackOrder(4)]
        public int Count { get; set; }
        [MemoryPackOrder(5)]
        public int TotalCount { get; set; }
        [MemoryPackOrder(6)]
        public string LogType { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Logs.Clear();
            this.Count = default;
            this.TotalCount = default;
            this.LogType = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.EditorLogRequest)]
    [ResponseType(nameof(EditorLogResponse))]
    public partial class EditorLogRequest : MessageObject, IRequest
    {
        public static EditorLogRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorLogRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Message { get; set; }
        [MemoryPackOrder(2)]
        public string LogType { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Message = default;
            this.LogType = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.EditorLogResponse)]
    public partial class EditorLogResponse : MessageObject, IResponse
    {
        public static EditorLogResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<EditorLogResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public bool Logged { get; set; }
        [MemoryPackOrder(4)]
        public string LogType { get; set; }
        [MemoryPackOrder(5)]
        public string LoggedMessage { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Logged = default;
            this.LogType = default;
            this.LoggedMessage = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.ScreenshotCaptureRequest)]
    [ResponseType(nameof(ScreenshotCaptureResponse))]
    public partial class ScreenshotCaptureRequest : MessageObject, IRequest
    {
        public static ScreenshotCaptureRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ScreenshotCaptureRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Target { get; set; }
        [MemoryPackOrder(2)]
        public string Format { get; set; }
        [MemoryPackOrder(3)]
        public int Quality { get; set; }
        [MemoryPackOrder(4)]
        public bool AllowEditMode { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Target = default;
            this.Format = default;
            this.Quality = default;
            this.AllowEditMode = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.ScreenshotCaptureResponse)]
    public partial class ScreenshotCaptureResponse : MessageObject, IResponse
    {
        public static ScreenshotCaptureResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ScreenshotCaptureResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public bool Captured { get; set; }
        [MemoryPackOrder(4)]
        public string Target { get; set; }
        [MemoryPackOrder(5)]
        public BridgeScreenshotInfo Screenshot { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Captured = default;
            this.Target = default;
            this.Screenshot = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameViewGetResolutionRequest)]
    [ResponseType(nameof(GameViewGetResolutionResponse))]
    public partial class GameViewGetResolutionRequest : MessageObject, IRequest
    {
        public static GameViewGetResolutionRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameViewGetResolutionRequest>(isFromPool);
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
    [Message(Opcode.GameViewGetResolutionResponse)]
    public partial class GameViewGetResolutionResponse : MessageObject, IResponse
    {
        public static GameViewGetResolutionResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameViewGetResolutionResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public BridgeGameViewResolution Resolution { get; set; }
        [MemoryPackOrder(4)]
        public int SelectedIndex { get; set; }
        [MemoryPackOrder(5)]
        public string SizeType { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Resolution = default;
            this.SelectedIndex = default;
            this.SizeType = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameViewListResolutionsRequest)]
    [ResponseType(nameof(GameViewListResolutionsResponse))]
    public partial class GameViewListResolutionsRequest : MessageObject, IRequest
    {
        public static GameViewListResolutionsRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameViewListResolutionsRequest>(isFromPool);
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
    [Message(Opcode.GameViewListResolutionsResponse)]
    public partial class GameViewListResolutionsResponse : MessageObject, IResponse
    {
        public static GameViewListResolutionsResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameViewListResolutionsResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public List<BridgeGameViewResolution> Resolutions { get; set; } = new();

        [MemoryPackOrder(4)]
        public int Count { get; set; }
        [MemoryPackOrder(5)]
        public int CurrentIndex { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Resolutions.Clear();
            this.Count = default;
            this.CurrentIndex = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameViewSetResolutionRequest)]
    [ResponseType(nameof(GameViewSetResolutionResponse))]
    public partial class GameViewSetResolutionRequest : MessageObject, IRequest
    {
        public static GameViewSetResolutionRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameViewSetResolutionRequest>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Width { get; set; }
        [MemoryPackOrder(2)]
        public int Height { get; set; }
        [MemoryPackOrder(3)]
        public string Label { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Width = default;
            this.Height = default;
            this.Label = default;

            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.GameViewSetResolutionResponse)]
    public partial class GameViewSetResolutionResponse : MessageObject, IResponse
    {
        public static GameViewSetResolutionResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<GameViewSetResolutionResponse>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public BridgeGameViewResolution Resolution { get; set; }
        [MemoryPackOrder(4)]
        public int SelectedIndex { get; set; }
        [MemoryPackOrder(5)]
        public bool WasAdded { get; set; }
        [MemoryPackOrder(6)]
        public string SizeType { get; set; }
        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Resolution = default;
            this.SelectedIndex = default;
            this.WasAdded = default;
            this.SizeType = default;

            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort Ping = 11101;
        public const ushort PingResponse = 11102;
        public const ushort HostState = 11103;
        public const ushort HostStateResponse = 11104;
        public const ushort Compile = 11105;
        public const ushort CompileResponse = 11106;
        public const ushort Reload = 11107;
        public const ushort ReloadResponse = 11108;
        public const ushort EnterPlay = 11109;
        public const ushort EnterPlayResponse = 11110;
        public const ushort ExitPlay = 11111;
        public const ushort ExitPlayResponse = 11112;
        public const ushort RegenProject = 11113;
        public const ushort RegenProjectResponse = 11114;
        public const ushort ErrorResponse = 11115;
        public const ushort Refresh = 11116;
        public const ushort RefreshResponse = 11117;
        public const ushort TestEcho = 11118;
        public const ushort TestEchoResponse = 11119;
        public const ushort BridgeVector2 = 11120;
        public const ushort BridgeVector3 = 11121;
        public const ushort BridgeQuaternion = 11122;
        public const ushort BridgeTransformInfo = 11123;
        public const ushort BridgeObjectInfo = 11124;
        public const ushort BridgeAssetInfo = 11125;
        public const ushort BridgeSceneNode = 11126;
        public const ushort BridgeComponentInfo = 11127;
        public const ushort BridgePropertyInfo = 11128;
        public const ushort BridgeConsoleLog = 11129;
        public const ushort BridgeGameViewResolution = 11130;
        public const ushort BridgeScreenshotInfo = 11131;
        public const ushort BridgeBatchStepResult = 11132;
        public const ushort BridgeTestResult = 11133;
        public const ushort AssetSearchRequest = 11134;
        public const ushort AssetSearchResponse = 11135;
        public const ushort AssetFindRequest = 11136;
        public const ushort AssetFindResponse = 11137;
        public const ushort AssetGetPathRequest = 11138;
        public const ushort AssetGetPathResponse = 11139;
        public const ushort AssetLoadRequest = 11140;
        public const ushort AssetLoadResponse = 11141;
        public const ushort AssetImportRequest = 11142;
        public const ushort AssetImportResponse = 11143;
        public const ushort AssetRefreshRequest = 11144;
        public const ushort AssetRefreshResponse = 11145;
        public const ushort AssetReadTextRequest = 11146;
        public const ushort AssetReadTextResponse = 11147;
        public const ushort SceneGetHierarchyRequest = 11148;
        public const ushort SceneGetHierarchyResponse = 11149;
        public const ushort SceneGetActiveRequest = 11150;
        public const ushort SceneGetActiveResponse = 11151;
        public const ushort SceneLoadRequest = 11152;
        public const ushort SceneLoadResponse = 11153;
        public const ushort SceneSaveRequest = 11154;
        public const ushort SceneSaveResponse = 11155;
        public const ushort SceneNewRequest = 11156;
        public const ushort SceneNewResponse = 11157;
        public const ushort SelectionGetRequest = 11158;
        public const ushort SelectionGetResponse = 11159;
        public const ushort SelectionSetRequest = 11160;
        public const ushort SelectionSetResponse = 11161;
        public const ushort SelectionAddRequest = 11162;
        public const ushort SelectionAddResponse = 11163;
        public const ushort SelectionRemoveRequest = 11164;
        public const ushort SelectionRemoveResponse = 11165;
        public const ushort SelectionClearRequest = 11166;
        public const ushort SelectionClearResponse = 11167;
        public const ushort UnityTestRunRequest = 11168;
        public const ushort UnityTestRunResponse = 11169;
        public const ushort ConsoleGetLogsRequest = 11170;
        public const ushort ConsoleGetLogsResponse = 11171;
        public const ushort EditorLogRequest = 11172;
        public const ushort EditorLogResponse = 11173;
        public const ushort ScreenshotCaptureRequest = 11174;
        public const ushort ScreenshotCaptureResponse = 11175;
        public const ushort GameViewGetResolutionRequest = 11176;
        public const ushort GameViewGetResolutionResponse = 11177;
        public const ushort GameViewListResolutionsRequest = 11178;
        public const ushort GameViewListResolutionsResponse = 11179;
        public const ushort GameViewSetResolutionRequest = 11180;
        public const ushort GameViewSetResolutionResponse = 11181;
    }
}