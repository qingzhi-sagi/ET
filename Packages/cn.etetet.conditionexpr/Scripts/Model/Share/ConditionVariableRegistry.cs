using System;
using System.Collections.Generic;
using System.Reflection;

namespace ET
{
    [CodeProcess]
    [AllowInstance]
    public class ConditionVariableRegistry : Singleton<ConditionVariableRegistry>, ISingletonAwake
    {
        private readonly Dictionary<string, Type> variableNodeTypes = new();
        private readonly Dictionary<string, int> numericTypes = new();

        public void Awake()
        {
            this.RegisterNumericTypes();
            
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof(ConditionVariableAttribute));
            foreach (Type type in types)
            {
                if (!typeof(BTCondition).IsAssignableFrom(type))
                {
                    throw new Exception($"condition variable node must inherit BTCondition: {type.FullName}");
                }

                object[] attrs = type.GetCustomAttributes(typeof(ConditionVariableAttribute), false);
                foreach (object attr in attrs)
                {
                    ConditionVariableAttribute conditionVariableAttribute = (ConditionVariableAttribute)attr;
                    if (string.IsNullOrWhiteSpace(conditionVariableAttribute.Variable))
                    {
                        throw new Exception($"condition variable is empty: {type.FullName}");
                    }

                    if (!this.variableNodeTypes.TryAdd(conditionVariableAttribute.Variable, type))
                    {
                        throw new Exception($"duplicate condition variable: {conditionVariableAttribute.Variable}");
                    }
                }
            }
        }

        private void RegisterNumericTypes()
        {
            FieldInfo[] fieldInfos = typeof(NumericType).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                if (fieldInfo.FieldType != typeof(int))
                {
                    continue;
                }

                string variable = fieldInfo.Name;
                this.numericTypes.Add(variable, (int)fieldInfo.GetValue(null));
                this.variableNodeTypes.Add(variable, typeof(BTNumericCompare));
            }
        }

        public bool TryGetNodeType(string variable, out Type nodeType)
        {
            return this.variableNodeTypes.TryGetValue(variable, out nodeType);
        }

        public bool TryGetNumericType(string variable, out int numericType)
        {
            return this.numericTypes.TryGetValue(variable, out numericType);
        }
    }
}
