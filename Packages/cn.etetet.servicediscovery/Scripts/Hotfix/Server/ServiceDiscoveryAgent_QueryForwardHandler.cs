using System;
using System.Collections.Generic;

namespace ET.Server
{
    [MessageHandler(SceneType.ServiceDiscoveryAgent)]
    public class ServiceDiscoveryAgent_QueryForwardHandler :
        MessageHandler<Scene, ServiceQueryRequest, ServiceQueryResponse>
    {
        protected override async ETTask Run(Scene scene, ServiceQueryRequest request, ServiceQueryResponse response)
        {
            if (!ServiceDiscoveryHelper.TryValidateMetadataMap(request.Filter, nameof(ServiceQueryRequest), nameof(request.Filter),
                    out string errorMessage))
            {
                ServiceDiscoveryErrorHelper.SetInvalidArgument(response, errorMessage);
                return;
            }

            ServiceDiscoveryAgent agent = scene.GetComponent<ServiceDiscoveryAgent>();
            EntityRef<ServiceDiscoveryAgent> agentRef = agent;
            string sceneName = scene.Name;
            try
            {
                List<ServiceInfo> services = await agent.QueryLocalByFilterAsync(request.Filter);
                agent = agentRef;
                if (agent == null)
                {
                    return;
                }

                foreach (ServiceInfo serviceInfo in services)
                {
                    response.Services.Add(serviceInfo.ToProto());
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
                Log.Warning($"ServiceDiscovery agent query forward failed scene: {sceneName} error: {e.Message}");
            }
        }
    }
}
