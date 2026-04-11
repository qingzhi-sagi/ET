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
        public const ushort TestEcho = 11201;
        public const ushort TestEchoResponse = 11202;
    }
}
