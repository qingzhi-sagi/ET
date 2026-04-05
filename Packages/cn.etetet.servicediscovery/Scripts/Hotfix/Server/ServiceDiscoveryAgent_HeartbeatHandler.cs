using System;

namespace ET.Server
{
    [MessageHandler(SceneType.ServiceDiscoveryAgent)]
    public class ServiceDiscoveryAgent_HeartbeatHandler :
        MessageHandler<Scene, ServiceHeartbeatRequest, ServiceHeartbeatResponse>
    {
        protected override async ETTask Run(Scene scene, ServiceHeartbeatRequest request, ServiceHeartbeatResponse response)
        {
            ServiceDiscoveryAgent agent = scene.GetComponent<ServiceDiscoveryAgent>();
            if (agent == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            if (!ServiceDiscoveryHelper.TryValidateRequiredText(request.SceneName, nameof(ServiceHeartbeatRequest),
                    nameof(request.SceneName), out string errorMessage))
            {
                ServiceDiscoveryErrorHelper.SetInvalidArgument(response, errorMessage);
                await ETTask.CompletedTask;
                return;
            }

            ServiceDiscoveryAgentProxyHeartbeat proxyHeartbeat = agent.GetComponent<ServiceDiscoveryAgentProxyHeartbeat>();
            if (proxyHeartbeat == null)
            {
                response.Error = ErrorCode.ERR_ComponentNotFound;
                response.Message = "proxy heartbeat component missing";
                return;
            }

            proxyHeartbeat.TouchProxyHeartbeat(request.SceneName);
            await ETTask.CompletedTask;
        }
    }
}
