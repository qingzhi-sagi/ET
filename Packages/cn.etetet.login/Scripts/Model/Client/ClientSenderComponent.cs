namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class ClientSenderComponent: Entity, IAwake, IDestroy
    {
        public long fiberId;

        public FiberInstanceId FiberInstanceId;
    }
}
