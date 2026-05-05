namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ProcessFiberAddressComponent : Entity, IAwake, IDestroy
    {
        public int SceneType;
        public FiberInstanceId FiberInstanceId;
    }
}
