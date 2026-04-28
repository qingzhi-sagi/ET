namespace ET.Test
{
    public class Conditionexpr_Compile_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty, nameof(Conditionexpr_Compile_Test));

            ConditionRoot root = ConditionExprCompiler.Compile("(HP >= 10 : 10001 || MP >= 100 : 10002) && Speed > 0 : 10003", 9999);

            if (root.Children.Count != 1)
            {
                Log.Console($"root child count error, actual: {root.Children.Count}");
                return 1;
            }

            if (root.Children[0] is not BTSequence sequence)
            {
                Log.Console($"root child type error, actual: {root.Children[0].GetType().Name}");
                return 2;
            }

            if (sequence.Children.Count != 2)
            {
                Log.Console($"sequence child count error, actual: {sequence.Children.Count}");
                return 3;
            }

            if (sequence.Children[0] is not BTSelector selector)
            {
                Log.Console($"first sequence child type error, actual: {sequence.Children[0].GetType().Name}");
                return 4;
            }

            if (selector.Children.Count != 2)
            {
                Log.Console($"selector child count error, actual: {selector.Children.Count}");
                return 5;
            }

            if (selector.Children[0] is not BTNumericCompare hpCompare)
            {
                Log.Console($"first selector child type error, actual: {selector.Children[0].GetType().Name}");
                return 6;
            }

            if (hpCompare.NumericType != NumericType.HP || hpCompare.Op != ConditionCompareOp.GreaterEqual || hpCompare.Value != 10 || hpCompare.ErrorCode != 10001)
            {
                Log.Console("hp compare data error");
                return 7;
            }

            if (selector.Children[1] is not BTNumericCompare mpCompare)
            {
                Log.Console($"second selector child type error, actual: {selector.Children[1].GetType().Name}");
                return 8;
            }

            if (mpCompare.NumericType != NumericType.MP || mpCompare.Op != ConditionCompareOp.GreaterEqual || mpCompare.Value != 100 || mpCompare.ErrorCode != 10002)
            {
                Log.Console("mp compare data error");
                return 9;
            }

            if (sequence.Children[1] is not BTNumericCompare speedCompare)
            {
                Log.Console($"second sequence child type error, actual: {sequence.Children[1].GetType().Name}");
                return 10;
            }

            if (speedCompare.NumericType != NumericType.Speed || speedCompare.Op != ConditionCompareOp.Greater || speedCompare.Value != 0 || speedCompare.ErrorCode != 10003)
            {
                Log.Console("speed compare data error");
                return 11;
            }

            ConditionVariableRegistry registry = ConditionVariableRegistry.Instance;
            if (!registry.TryGetNodeType("HP", out System.Type hpNodeType) || hpNodeType != typeof(BTNumericCompare))
            {
                Log.Console("hp node type register error");
                return 12;
            }

            if (!typeof(BTCondition).IsAssignableFrom(hpNodeType))
            {
                Log.Console("hp node type condition base error");
                return 13;
            }

            if (typeof(BTNumericCompare).BaseType != typeof(BTCondition))
            {
                Log.Console("numeric compare base type error");
                return 14;
            }

            return ErrorCode.ERR_Success;
        }
    }
}
