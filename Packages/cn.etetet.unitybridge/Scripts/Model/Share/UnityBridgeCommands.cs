using System;

namespace ET
{
    [EnableClass]
    public sealed class UnityBridgeRequestEnvelope : Object
    {
        public int RpcId { get; set; }

        public string IdempotencyKey { get; set; }

        public int TimeoutMs { get; set; }

        public string CommandJson { get; set; }
    }

    [EnableClass]
    internal sealed class UnityBridgeDeferredResponse : Object, IResponse
    {
        public int Error { get; set; }

        public string Message { get; set; }

        public int RpcId { get; set; }
    }

    [EnableClass]
    public interface IUnityBridgeHandler
    {
        Type RequestType { get; }

        Type ResponseType { get; }

        ETTask<IResponse> Handle(IRequest request);
    }

    [EnableClass]
    public interface IUnityBridgeDeferredHandler
    {
        ETTask<IResponse> Deferred(IRequest request, long startedAt);
    }

    public static class UnityBridgeResponseHelper
    {
        public static IResponse CreateDeferredResponse()
        {
            return new UnityBridgeDeferredResponse();
        }

        public static ErrorResponse CreateErrorResponse(int rpcId, string commandType, int error, string message)
        {
            ErrorResponse response = ErrorResponse.Create();
            response.RpcId = rpcId;
            response.Error = error;
            response.Message = message ?? string.Empty;
            response.Command = commandType ?? string.Empty;
            return response;
        }

        public static bool IsDeferredResponse(IResponse response)
        {
            return response is UnityBridgeDeferredResponse;
        }
    }
}
