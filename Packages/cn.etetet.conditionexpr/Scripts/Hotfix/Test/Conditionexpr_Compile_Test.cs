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

            ConditionRoot namedRoot = ConditionExprCompiler.Compile("Unit1.HP > 0 || Unit2.MP < 100", 9999);
            if (namedRoot.Children.Count != 1 || namedRoot.Children[0] is not BTSelector namedSelector || namedSelector.Children.Count != 2)
            {
                Log.Console("named owner root structure error");
                return 15;
            }

            if (namedSelector.Children[0] is not BTNumericCompare namedHpCompare)
            {
                Log.Console($"named owner first child type error, actual: {namedSelector.Children[0].GetType().Name}");
                return 16;
            }

            if (namedHpCompare.OwnerKey != "Unit1" || namedHpCompare.NumericType != NumericType.HP || namedHpCompare.Op != ConditionCompareOp.Greater || namedHpCompare.Value != 0)
            {
                Log.Console("named owner hp compare data error");
                return 17;
            }

            if (namedSelector.Children[1] is not BTNumericCompare namedMpCompare)
            {
                Log.Console($"named owner second child type error, actual: {namedSelector.Children[1].GetType().Name}");
                return 18;
            }

            if (namedMpCompare.OwnerKey != "Unit2" || namedMpCompare.NumericType != NumericType.MP || namedMpCompare.Op != ConditionCompareOp.Less || namedMpCompare.Value != 100)
            {
                Log.Console("named owner mp compare data error");
                return 19;
            }

            RegisterParamCompareNode("Friend1");
            RegisterParamCompareNode("Friend2");
            ConditionRoot paramRoot = ConditionExprCompiler.Compile("Friend1(HP) > 0 || Friend2(HP, MP) < 100", 9999);
            if (paramRoot.Children.Count != 1 || paramRoot.Children[0] is not BTSelector paramSelector || paramSelector.Children.Count != 2)
            {
                Log.Console("param root structure error");
                return 20;
            }

            if (paramSelector.Children[0] is not Conditionexpr_ParamCompareNode friend1Compare)
            {
                Log.Console($"param first child type error, actual: {paramSelector.Children[0].GetType().Name}");
                return 21;
            }

            if (friend1Compare.Params.Length != 1 || friend1Compare.Params[0] != "HP" || friend1Compare.Op != ConditionCompareOp.Greater || friend1Compare.Value != 0)
            {
                Log.Console("single param node data error");
                return 22;
            }

            if (paramSelector.Children[1] is not Conditionexpr_ParamCompareNode friend2Compare)
            {
                Log.Console($"param second child type error, actual: {paramSelector.Children[1].GetType().Name}");
                return 23;
            }

            if (friend2Compare.Params.Length != 2 || friend2Compare.Params[0] != "HP" || friend2Compare.Params[1] != "MP" ||
                friend2Compare.Op != ConditionCompareOp.Less || friend2Compare.Value != 100)
            {
                Log.Console("multiple param node data error");
                return 24;
            }

            try
            {
                ConditionExprCompiler.Compile("HP(Friend1) > 0", 9999);
                Log.Console("numeric node params should fail");
                return 25;
            }
            catch (System.Exception e)
            {
                if (!e.Message.Contains("params field not found"))
                {
                    Log.Console($"numeric node params fail message error: {e.Message}");
                    return 26;
                }
            }

            try
            {
                ConditionExprCompiler.Compile("Unit1.Friend1(HP) > 0", 9999);
                Log.Console("node owner key without field should fail");
                return 27;
            }
            catch (System.Exception e)
            {
                if (!e.Message.Contains("owner key field not found"))
                {
                    Log.Console($"owner key field fail message error: {e.Message}");
                    return 28;
                }
            }

            return ErrorCode.ERR_Success;
        }

        private static void RegisterParamCompareNode(string variable)
        {
            System.Reflection.FieldInfo fieldInfo = typeof(ConditionVariableRegistry).GetField("variableNodeTypes",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            System.Collections.Generic.Dictionary<string, System.Type> nodeTypes =
                    (System.Collections.Generic.Dictionary<string, System.Type>)fieldInfo.GetValue(ConditionVariableRegistry.Instance);
            nodeTypes[variable] = typeof(Conditionexpr_ParamCompareNode);
        }
    }
}
