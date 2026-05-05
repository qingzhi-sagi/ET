using System.Net;

namespace ET.Server
{
    [Invoke(SceneType.NetInner)]
    public class FiberInit_NetInner: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Fiber fiber = fiberInit.Fiber;
            Scene root = fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessOuterSender, IPEndPoint>(root.GetSingleton<AddressSingleton>().InnerAddress);
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<ProcessFiberAddressComponent>();

            await ETTask.CompletedTask;
        }
    }
}
