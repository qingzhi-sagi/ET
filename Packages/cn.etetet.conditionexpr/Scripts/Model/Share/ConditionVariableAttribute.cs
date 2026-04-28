using System;

namespace ET
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ConditionVariableAttribute : BaseAttribute
    {
        public string Variable { get; }

        public ConditionVariableAttribute(string variable)
        {
            this.Variable = variable;
        }
    }
}
