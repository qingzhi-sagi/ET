using System;

namespace ET.Server
{
    [MessageHandler(SceneType.ServiceDiscoveryAgent)]
    public class ServiceDiscoveryAgent_SubscribeForwardHandler :
        MessageHandler<Scene, ServiceSubscribeRequest, ServiceSubscribeResponse>
    {
        protected override async ETTask Run(Scene scene, ServiceSubscribeRequest request, ServiceSubscribeResponse response)
        {
            if (!ServiceDiscoveryHelper.TryValidateRequiredText(request.SceneName, nameof(ServiceSubscribeRequest),
                    nameof(request.SceneName), out string errorMessage))
            {
                ServiceDiscoveryErrorHelper.SetInvalidArgument(response, errorMessage);
                return;
            }

            if (request.SubscriberActorId == default)
            {
                response.Error = ErrorCode.ERR_SubscriberActorIdRequired;
                response.Message = $"{nameof(ServiceSubscribeRequest)} invalid: SubscriberActorId is empty.";
                return;
            }

            if (!ServiceDiscoveryHelper.TryValidateRequiredText(request.FilterName, nameof(ServiceSubscribeRequest),
                    nameof(request.FilterName), out errorMessage))
            {
                ServiceDiscoveryErrorHelper.SetInvalidArgument(response, errorMessage);
                return;
            }

            if (!ServiceDiscoveryHelper.TryValidateMetadataMap(request.FilterMetadata, nameof(ServiceSubscribeRequest),
                    nameof(request.FilterMetadata), out errorMessage))
            {
                ServiceDiscoveryErrorHelper.SetInvalidArgument(response, errorMessage);
                return;
            }

            ServiceDiscoveryAgent agent = scene.GetComponent<ServiceDiscoveryAgent>();
            EntityRef<ServiceDiscoveryAgent> agentRef = agent;
            string sceneName = scene.Name;
            try
            {
                await agent.EnsureReadyAsync(nameof(ServiceSubscribeRequest));
                agent = agentRef;
                if (agent == null)
                {
                    return;
                }

                int error = agent.SubscribeProxyServiceChange(request.SceneName, request.SubscriberActorId, request.FilterName,
                    request.FilterMetadata, response.Services);
                if (error != ErrorCode.ERR_Success)
                {
                    response.Error = error;
                    response.Message = nameof(ErrorCode.ERR_SubscriberActorIdRequired);
                }
            }
            catch (RpcException rpcException)
            {
                response.Error = rpcException.Error;
                response.Message = rpcException.Message;
            }
            catch (Exception e)
            {
                agent = agentRef;
                if (agent == null)
                {
                    return;
                }

                ServiceDiscoveryErrorHelper.SetInternalFailure(response, e.Message);
                Log.Warning($"ServiceDiscovery agent subscribe forward failed scene: {sceneName} error: {e.Message}");
            }
        }
    }
}
