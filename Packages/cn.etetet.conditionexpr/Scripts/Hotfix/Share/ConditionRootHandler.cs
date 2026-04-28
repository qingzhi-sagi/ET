namespace ET
{
    public class ConditionRootHandler : ABTHandler<ConditionRoot>
    {
        protected override int Run(ConditionRoot node, BTEnv env)
        {
            if (node.Children == null || node.Children.Count == 0)
            {
                return ErrorCode.ERR_Success;
            }

            if (node.Children.Count > 1)
            {
                throw new System.Exception($"condition root child count error: {node.Children.Count}");
            }

            return BTDispatcher.Instance.Handle(node.Children[0], env);
        }
    }
}
