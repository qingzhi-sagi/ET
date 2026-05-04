namespace ET.Test
{
    public class InheritableFiberSingletonContract :
            Singleton<InheritableFiberSingletonContract>,
            ISingletonAwake<int>,
            IInheritableSingleton
    {
        public int Value;

        public void Awake(int value)
        {
            this.Value = value;
        }
    }
}
