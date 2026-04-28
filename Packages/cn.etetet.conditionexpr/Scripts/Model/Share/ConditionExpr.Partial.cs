namespace ET
{
    public sealed partial class ConditionExpr
    {
        public ConditionRoot Root { get; private set; }

        partial void EndInit()
        {
            this.Root = ConditionExprCompiler.Compile(this.Expr, this.ErrorCode);
        }
    }
}
