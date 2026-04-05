using System;

namespace ET.Server
{
    [MessageHandler(SceneType.ServiceDiscoveryAgent)]
    public class ServiceDiscoveryAgent_UnsubscribeForwardHandler :
        MessageHandler<Scene, ServiceUnsubscribeRequest, ServiceUnsubscribeResponse>
    {
        protected override async ETTask Run(Scene scene, ServiceUnsubscribeRequest request, ServiceUnsubscribeResponse response)
        {
            if (!ServiceDiscoveryHelper.TryValidateRequiredText(request.SceneName, nameof(ServiceUnsubscribeRequest),
                    nameof(request.SceneName), out string errorMessage))
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

                agent.UnsubscribeProxyServiceChange(request.SceneName);
                await ETTask.CompletedTask;
            }
            catch (Exception e)
            {
                ServiceDiscoveryErrorHelper.SetInternalFailure(response, e.Message);
                Log.Warning($"ServiceDiscovery agent unsubscribe forward failed scene: {sceneName} error: {e.Message}");
            }
        }
    }
}
