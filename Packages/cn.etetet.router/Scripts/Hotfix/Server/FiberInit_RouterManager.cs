using System.Runtime.InteropServices;

namespace ET.Server
{
    [Invoke(SceneType.RouterManager)]
    public class FiberInit_RouterManager: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();
            
            int outerPort = AddressHelper.GetSceneOuterPort(root.Fiber(), root.Name.GetSceneConfigName());
            
            string httpHost = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "+" : "*";
            root.AddComponent<HttpComponent, string>($"http://{httpHost}:{outerPort}/");

            ServiceDiscoveryProxy serviceDiscoveryProxy = root.AddComponent<ServiceDiscoveryProxy>();
            EntityRef<ServiceDiscoveryProxy> serviceDiscoveryProxyComponentRef = serviceDiscoveryProxy;
            
            // Test场景下动态zone不会创建对应Agent，跳过注册避免初始化失败。
            if (Options.Instance.SceneName != SceneTypeSingleton.Instance.GetSceneName(SceneType.Test))
            {
                serviceDiscoveryProxy = serviceDiscoveryProxyComponentRef;
                await serviceDiscoveryProxy.RegisterToServiceDiscovery();
            }
            // 订阅Router
            serviceDiscoveryProxy = serviceDiscoveryProxyComponentRef;
            await serviceDiscoveryProxy.SubscribeServiceChange("Router", 
                new StringKV()
                {
                    {ServiceMetaKey.SceneType, SceneTypeSingleton.Instance.GetSceneName(SceneType.Router)},
                });
            
            // 订阅Realm
            serviceDiscoveryProxy = serviceDiscoveryProxyComponentRef;
            await serviceDiscoveryProxy.SubscribeServiceChange("Realm", 
                new StringKV()
                {
                    {ServiceMetaKey.SceneType, SceneTypeSingleton.Instance.GetSceneName(SceneType.Realm)},
                });
        }
    }
}
