namespace ET.Server
{
    [Invoke(SceneType.Location)]
    public class FiberInit_Location: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();
            root.AddComponent<DBManagerComponent>();
            // location会把LocationComponent.Id作为PriorityId注册到服务发现，所以这个Id需要生成一次
            LocationComponent locationComponent = root.AddComponentWithId<LocationComponent>(IdGenerater.Instance.GenerateId());
            EntityRef<LocationComponent> locationComponentRef = locationComponent;
            
            // 注册服务发现，并订阅 location 服务变化，主节点按稳定排序规则判定。
            string sceneName = root.Name;
            int zone = root.Zone();
            ServiceDiscoveryProxy serviceDiscoveryProxy = root.AddComponent<ServiceDiscoveryProxy>();
            EntityRef<ServiceDiscoveryProxy> serviceDiscoveryProxyRef = serviceDiscoveryProxy;
            await serviceDiscoveryProxy.RegisterToServiceDiscovery(new StringKV
            {
                { ServiceMetaKey.PriorityId, locationComponent.Id.ToString() }
            });

            string locationSceneType = SceneTypeSingleton.Instance.GetSceneName(SceneType.Location);

            serviceDiscoveryProxy = serviceDiscoveryProxyRef;
            await serviceDiscoveryProxy.SubscribeServiceChange(locationSceneType, new StringKV
            {
                { ServiceMetaKey.SceneType, locationSceneType }
            });

            locationComponent = locationComponentRef;
            locationComponent.RefreshPrimaryState();

            Log.Info($"location node registered scene: {sceneName} zone: {zone} priorityId: {locationComponent.Id}");
        }
    }
}
