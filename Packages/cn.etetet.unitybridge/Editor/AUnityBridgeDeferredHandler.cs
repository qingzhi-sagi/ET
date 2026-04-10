namespace ET
{
    internal abstract class AUnityBridgeDeferredHandler<TRequest, TResponse> :
            AUnityBridgeHandler<TRequest, TResponse>, IUnityBridgeDeferredHandler
            where TRequest : class, IRequest
            where TResponse : class, IResponse
    {
        public sealed override async ETTask<IResponse> Handle(IRequest request)
        {
            TRequest command = (TRequest)request;

            if (!UnityBridgeDeferredRuntime.TryCreatePendingCurrent(command, out IResponse createError))
            {
                return createError;
            }

            try
            {
                await this.Run(command);
                return UnityBridgeResponseHelper.CreateDeferredResponse();
            }
            catch (System.Exception e)
            {
                UnityBridgeDeferredRuntime.RollbackCurrent();
                return this.CreateErrorResponse(command, UnityBridgeErrorCode.HandlerFail, e.ToString());
            }
        }

        public async ETTask<IResponse> Deferred(IRequest request, long startedAt)
        {
            await ETTask.CompletedTask;
            return this.Deferred((TRequest)request, startedAt);
        }

        protected abstract TResponse Deferred(TRequest command, long startedAt);
    }
}
