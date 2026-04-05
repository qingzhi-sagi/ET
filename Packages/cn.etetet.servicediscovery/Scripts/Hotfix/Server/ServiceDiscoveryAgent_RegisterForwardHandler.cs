using System;

namespace ET.Server
{
    [MessageHandler(SceneType.ServiceDiscoveryAgent)]
    public class ServiceDiscoveryAgent_RegisterForwardHandler :
        MessageHandler<Scene, ServiceRegisterRequest, ServiceRegisterResponse>
    {
        protected override async ETTask Run(Scene scene, ServiceRegisterRequest request, ServiceRegisterResponse response)
        {
            if (!ServiceDiscoveryHelper.TryValidateRequiredText(request.SceneName, nameof(ServiceRegisterRequest),
                    nameof(request.SceneName), out string errorMessage))
            {
                ServiceDiscoveryErrorHelper.SetInvalidArgument(response, errorMessage);
                return;
            }

            if (!ServiceDiscoveryHelper.TryValidateRequiredActorId(request.ActorId, nameof(ServiceRegisterRequest),
                    nameof(request.ActorId), out errorMessage))
            {
                ServiceDiscoveryErrorHelper.SetInvalidArgument(response, errorMessage);
                return;
            }

            if (!ServiceDiscoveryHelper.TryValidateMetadataMap(request.Metadata, nameof(ServiceRegisterRequest), nameof(request.Metadata),
                    out errorMessage))
            {
                ServiceDiscoveryErrorHelper.SetInvalidArgument(response, errorMessage);
                return;
            }

            ServiceDiscoveryAgent agent = scene.GetComponent<ServiceDiscoveryAgent>();
            string sceneName = scene.Name;
            try
            {
                if (agent == null)
                {
                    ServiceDiscoveryErrorHelper.SetInternalFailure(response, "service discovery agent missing");
                    return;
                }

                agent.UpsertLocalServiceAfterRegister(request.SceneName, request.ActorId, request.Metadata);

                ServiceDiscoveryAgentProxyHeartbeat proxyHeartbeat = agent.GetComponent<ServiceDiscoveryAgentProxyHeartbeat>();
                proxyHeartbeat?.TouchProxyHeartbeat(request.SceneName, true);

                await ETTask.CompletedTask;
            }
            catch (Exception e)
            {
                ServiceDiscoveryErrorHelper.SetInternalFailure(response, e.Message);
                Log.Warning($"ServiceDiscovery agent register forward failed scene: {sceneName} error: {e.Message}");
            }
        }
    }
}
