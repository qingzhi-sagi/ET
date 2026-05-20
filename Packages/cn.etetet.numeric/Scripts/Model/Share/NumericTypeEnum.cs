using System;

namespace ET
{
    public class NumericTypeEnum: EnumSingleton<NumericTypeEnum>, ISingletonAwake
    {
        public void Awake()
        {
            foreach (NumericType numericType in Enum.GetValues(typeof(NumericType)))
            {
                this.enumValueString.Add((int)numericType, numericType.ToString());
            }
        }
    }
}
