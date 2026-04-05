using System;
using System.Collections.Generic;

namespace ET.Server
{
    [MessageHandler(SceneType.ServiceDiscovery)]
    public class ServiceAgentRegisterRequestHandler : MessageHandler<Scene, ServiceAgentRegisterRequest, ServiceAgentRegisterResponse>
    {
        protected override async ETTask Run(Scene scene, ServiceAgentRegisterRequest request, ServiceAgentRegisterResponse response)
        {
            ServiceDiscovery serviceDiscovery = scene.GetComponent<ServiceDiscovery>();
            EntityRef<ServiceDiscovery> serviceDiscoveryRef = serviceDiscovery;
            try
            {
                bool isMaster = await serviceDiscovery.EnsureActiveMasterWithFenceAsync();
                serviceDiscovery = serviceDiscoveryRef;
                if (serviceDiscovery == null)
                {
                    return;
                }

                if (!isMaster)
                {
                    ServiceDiscoveryErrorHelper.SetNotWritableMaster(response, serviceDiscovery);
                    return;
                }

                if (!ServiceDiscoveryHelper.TryValidateRequiredActorId(request.AgentActorId, nameof(ServiceAgentRegisterRequest),
                        nameof(request.AgentActorId), out string errorMessage))
                {
                    ServiceDiscoveryErrorHelper.SetInvalidArgument(response, errorMessage);
                    return;
                }

                if (!ServiceDiscoveryHelper.TryValidateAgentLocalServices(request.LocalServices, request.AgentActorId,
                        nameof(ServiceAgentRegisterRequest), out errorMessage))
                {
                    ServiceDiscoveryErrorHelper.SetInvalidArgument(response, errorMessage);
                    return;
                }

                List<ServiceInfoProto> snapshot = await serviceDiscovery.RegisterAgentAsync(request.AgentActorId, request.LocalServices);
                serviceDiscovery = serviceDiscoveryRef;
                if (serviceDiscovery == null)
                {
                    return;
                }

                foreach (ServiceInfoProto serviceInfoProto in snapshot)
                {
                    if (serviceInfoProto != null)
                    {
                        response.Services.Add(serviceInfoProto);
                    }
                }
            }
            catch (Exception e)
            {
                serviceDiscovery = serviceDiscoveryRef;
                if (serviceDiscovery == null)
                {
                    return;
                }

                ServiceDiscoveryErrorHelper.SetPersistenceFailed(response, e.Message);
                Log.Error($"Service agent register failed agent: {request.AgentActorId} error: {e}");
            }
        }
    }

    [MessageHandler(SceneType.ServiceDiscovery)]
    public class ServiceHeartbeatRequestHandler : MessageHandler<Scene, ServiceHeartbeatRequest, ServiceHeartbeatResponse>
    {
        protected override async ETTask Run(Scene scene, ServiceHeartbeatRequest request, ServiceHeartbeatResponse response)
        {
            ServiceDiscovery serviceDiscovery = scene.GetComponent<ServiceDiscovery>();
            EntityRef<ServiceDiscovery> serviceDiscoveryRef = serviceDiscovery;
            try
            {
                bool isMaster = await serviceDiscovery.EnsureActiveMasterWithFenceAsync();
                serviceDiscovery = serviceDiscoveryRef;
                if (serviceDiscovery == null)
                {
                    return;
                }

                if (!isMaster)
                {
                    ServiceDiscoveryErrorHelper.SetNotWritableMaster(response, serviceDiscovery);
                    return;
                }

                if (!ServiceDiscoveryHelper.TryValidateRequiredActorId(request.AgentActorId, nameof(ServiceHeartbeatRequest),
                        nameof(request.AgentActorId), out string errorMessage))
                {
                    ServiceDiscoveryErrorHelper.SetInvalidArgument(response, errorMessage);
                    return;
                }

                await serviceDiscovery.UpdateAgentHeartbeatAsync(request.AgentActorId);
            }
            catch (Exception e)
            {
                serviceDiscovery = serviceDiscoveryRef;
                if (serviceDiscovery == null)
                {
                    return;
                }
                ServiceDiscoveryErrorHelper.SetPersistenceFailed(response, e.Message);
                Log.Error($"Service heartbeat update failed agent: {request.AgentActorId} scene: {request.SceneName} error: {e}");
            }
        }
    }
}
