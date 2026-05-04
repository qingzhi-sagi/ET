namespace ET.Server
{
    [Invoke(SceneType.ServiceDiscoveryAgent)]
    public class FiberInit_ServiceDiscoveryAgent : AInvokeHandler<FiberInit, ETTask>
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

            // Agent 启动不阻塞等待向主节点注册完成：
            // 只拉起后台 bootstrap 协程，后续由协程完成 master 解析与注册。
            root.AddComponent<ServiceDiscoveryAgent>();
            await ETTask.CompletedTask;
        }
    }
}
