namespace ET.Test
{
    public class Conditionexpr_Run_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty, nameof(Conditionexpr_Run_Test));
            Scene scene = scope.TestFiber.Root;

            UnitComponent unitComponent = scene.AddComponent<UnitComponent>();
            Unit unit = unitComponent.AddChildWithId<Unit, int>(1001, 0);
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();

            ConditionRoot root = ConditionExprCompiler.Compile("(HP >= 10 : 10001 || MP >= 100 : 10002) && Speed > 0 : 10003", 9999);

            BTEnv env = BTEnv.Create(scene, unit.Id);
            try
            {
                env.AddEntity(ConditionExprEnvKeys.Unit, unit);

                numericComponent.NumericDic[NumericType.HP] = 5;
                numericComponent.NumericDic[NumericType.MP] = 50;
                numericComponent.NumericDic[NumericType.Speed] = 1;

                int ret = BTHelper.RunTree(root, env);
                if (ret != 10001)
                {
                    Log.Console($"both selector branches failed error, expected: 10001, actual: {ret}");
                    return 1;
                }

                numericComponent.NumericDic[NumericType.MP] = 100;
                ret = BTHelper.RunTree(root, env);
                if (ret != ErrorCode.ERR_Success)
                {
                    Log.Console($"mp branch success error, expected: 0, actual: {ret}");
                    return 2;
                }

                numericComponent.NumericDic[NumericType.HP] = 10;
                numericComponent.NumericDic[NumericType.Speed] = 0;
                ret = BTHelper.RunTree(root, env);
                if (ret != 10003)
                {
                    Log.Console($"sequence second child failed error, expected: 10003, actual: {ret}");
                    return 3;
                }
            }
            finally
            {
                env.Dispose();
            }

            return ErrorCode.ERR_Success;
        }
    }
}
