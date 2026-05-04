namespace ET
{
    public class ForceParentSchedulerSingleton :
            Singleton<ForceParentSchedulerSingleton>,
            ISingletonAwake,
            IInheritableSingleton
    {
        public void Awake()
        {
        }
    }
}
