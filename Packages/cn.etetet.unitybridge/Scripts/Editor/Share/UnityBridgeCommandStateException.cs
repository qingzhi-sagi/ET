using System;

namespace ET
{
    internal sealed class UnityBridgeCommandStateException : Exception
    {
        public UnityBridgeCommandStateException(int error, string message) : base(message)
        {
            this.Error = error;
        }

        public int Error { get; }
    }
}
