namespace ET.Test
{
    public class Conditionexpr_LubanBean_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty, nameof(Conditionexpr_LubanBean_Test));

            ConditionExpr conditionExpr = new("HP >= 10 : 10001", 9999, "test");
            if (conditionExpr.Root == null)
            {
                Log.Console("condition expr root is null");
                return 1;
            }

            if (conditionExpr.Root.Children.Count != 1)
            {
                Log.Console($"condition expr root child count error, actual: {conditionExpr.Root.Children.Count}");
                return 2;
            }

            if (conditionExpr.Root.Children[0] is not BTNumericCompare hpCompare)
            {
                Log.Console($"condition expr root child type error, actual: {conditionExpr.Root.Children[0].GetType().Name}");
                return 3;
            }

            if (hpCompare.NumericType != NumericType.HP || hpCompare.ErrorCode != 10001)
            {
                Log.Console("condition expr numeric compare data error");
                return 4;
            }

            return ErrorCode.ERR_Success;
        }
    }
}
