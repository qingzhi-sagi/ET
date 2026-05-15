namespace ET
{
    public class BTNumericCompareHandler : ABTHandler<BTNumericCompare>
    {
        protected override int Run(BTNumericCompare node, BTEnv env)
        {
            Unit owner = env.GetEntity<Unit>(node.OwnerKey);
            NumericComponent numericComponent = owner.NumericComponent;
            long left = numericComponent.GetAsLong(node.NumericType);
            return ConditionCompareHelper.Compare(left, node.Op, node.Value) ? ErrorCode.ERR_Success : node.ErrorCode;
        }
    }
}
