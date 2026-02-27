namespace ET
{
    public class NumericTypeEnum: EnumSingleton<NumericTypeEnum>, ISingletonAwake
    {
        public void Awake()
        {
            Init(typeof(NumericType));
        }
    }
}
