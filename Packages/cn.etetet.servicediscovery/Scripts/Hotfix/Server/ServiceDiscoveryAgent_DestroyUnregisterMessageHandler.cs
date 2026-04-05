using System;

namespace ET.Server
{
    [MessageHandler(SceneType.ServiceDiscoveryAgent)]
    public class ServiceDiscoveryAgent_DestroyUnregisterMessageHandler:
        MessageHandler<Scene, ServiceProxyDestroyUnregisterMessage>
    {
        protected override async ETTask Run(Scene scene, ServiceProxyDestroyUnregisterMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.SceneName))
            {
                Log.Warning($"{nameof(ServiceProxyDestroyUnregisterMessage)} invalid: SceneName is empty.");
                return;
            }

            ServiceDiscoveryAgent agent = scene.GetComponent<ServiceDiscoveryAgent>();
            if (agent == null)
            {
                Log.Warning(
                    $"ServiceDiscovery agent destroy unregister ignored because agent component is null scene: {scene.Name}");
                return;
            }

            EntityRef<ServiceDiscoveryAgent> agentRef = agent;
            string sceneName = scene.Name;
            string proxySceneName = message.SceneName;
            try
            {
                agent = agentRef;
                if (agent == null)
                {
                    return;
                }

                // Destroy阶段不再接收订阅通知，先从Agent侧移除订阅关系。
                agent.UnsubscribeProxyServiceChange(proxySceneName);

                agent.RemoveLocalServiceAfterUnregister(proxySceneName);
                ServiceDiscoveryAgentProxyHeartbeat proxyHeartbeat = agent.GetComponent<ServiceDiscoveryAgentProxyHeartbeat>();
                proxyHeartbeat?.RemoveProxyHeartbeat(proxySceneName);

                await ETTask.CompletedTask;
            }
            catch (Exception e)
            {
                agent = agentRef;
                if (agent != null)
                {
                    Log.Warning(
                        $"ServiceDiscovery agent destroy unregister failed scene: {sceneName} proxyScene: {proxySceneName} error: {e.Message}");
                }
            }
        }
    }
}
