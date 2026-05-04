namespace ET.Server
{
    public class ServiceDiscoveryBootstrapSingleton :
            Singleton<ServiceDiscoveryBootstrapSingleton>,
            ISingletonAwake,
            ISingletonAwake<ActorId>,
            IInheritableSingleton
    {
        public ActorId MasterActorId { get; private set; }

        public void Awake()
        {
        }

        public void Awake(ActorId masterActorId)
        {
            this.SetMasterActorId(masterActorId);
        }

        public void SetMasterActorId(ActorId masterActorId)
        {
            this.MasterActorId = masterActorId;
        }
    }
}
