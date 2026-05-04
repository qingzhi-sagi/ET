namespace ET.Server
{
    /// <summary>
    /// 服务发现Fiber初始化处理器
    /// </summary>
    [Invoke(SceneType.ServiceDiscovery)]
    public class FiberInit_ServiceDiscovery : AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Fiber fiber = fiberInit.Fiber;
            Scene root = fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();

            if (fiber.GetSingleton<ServiceDiscoveryBootstrapSingleton>() == null)
            {
                root.AddComponent<DBManagerComponent>();
            }
            
            // 添加服务发现核心组件
            ServiceDiscovery serviceDiscovery = root.AddComponent<ServiceDiscovery>();
            await serviceDiscovery.InitializeAsync();
        }
    }
}
