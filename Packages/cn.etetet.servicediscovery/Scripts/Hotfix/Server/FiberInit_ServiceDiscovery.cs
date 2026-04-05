namespace ET.Server
{
    /// <summary>
    /// 服务发现Fiber初始化处理器
    /// 注意FiberId=-2
    /// </summary>
    [Invoke(SceneType.ServiceDiscovery)]
    public class FiberInit_ServiceDiscovery : AInvokeHandler<FiberInit, ETTask>
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
            
            // 添加服务发现核心组件
            ServiceDiscovery serviceDiscovery = root.AddComponent<ServiceDiscovery>();
            await serviceDiscovery.InitializeAsync();
        }
    }
}
