namespace ET
{
    public class BTNumericCompareHandler : ABTHandler<BTNumericCompare>
    {
        protected override int Run(BTNumericCompare node, BTEnv env)
        {
            Unit unit = env.GetEntity<Unit>(ConditionExprEnvKeys.Unit);
            NumericComponent numericComponent = unit.NumericComponent;
            long left = numericComponent.GetAsLong(node.NumericType);
            return ConditionCompareHelper.Compare(left, node.Op, node.Value) ? ErrorCode.ERR_Success : node.ErrorCode;
        }
    }
}
