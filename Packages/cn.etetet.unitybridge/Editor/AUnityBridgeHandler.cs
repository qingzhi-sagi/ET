using System;

namespace ET
{
    [EnableClass]
    public abstract class AUnityBridgeHandler<TRequest, TResponse> : IUnityBridgeHandler
            where TRequest : class, IRequest
            where TResponse : class, IResponse
    {
        public Type RequestType => typeof(TRequest);

        public Type ResponseType => typeof(TResponse);

        public virtual async ETTask<IResponse> Handle(IRequest request)
        {
            return await this.Run((TRequest)request);
        }

        protected abstract ETTask<IResponse> Run(TRequest command);

        protected IResponse CreateErrorResponse(IRequest request, int error, string message)
        {
            return UnityBridgeResponseHelper.CreateErrorResponse(
                request?.RpcId ?? 0,
                typeof(TRequest).Name,
                error,
                message);
        }
    }
}