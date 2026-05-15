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
            Unit unit2 = unitComponent.AddChildWithId<Unit, int>(1002, 0);
            NumericComponent numericComponent2 = unit2.AddComponent<NumericComponent>();

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

            ConditionRoot namedRoot = ConditionExprCompiler.Compile("Unit1.HP > 0 : 20001 || Unit2.MP < 100 : 20002", 9999);
            BTEnv namedEnv = BTEnv.Create(scene, unit.Id);
            try
            {
                namedEnv.AddEntity("Unit1", unit);
                namedEnv.AddEntity("Unit2", unit2);

                numericComponent.NumericDic[NumericType.HP] = 0;
                numericComponent.NumericDic[NumericType.MP] = 150;
                numericComponent2.NumericDic[NumericType.MP] = 150;

                int ret = BTHelper.RunTree(namedRoot, namedEnv);
                if (ret != 20001)
                {
                    Log.Console($"named owner both branches failed error, expected: 20001, actual: {ret}");
                    return 4;
                }

                numericComponent.NumericDic[NumericType.HP] = 1;
                ret = BTHelper.RunTree(namedRoot, namedEnv);
                if (ret != ErrorCode.ERR_Success)
                {
                    Log.Console($"named owner first branch success error, expected: 0, actual: {ret}");
                    return 5;
                }

                numericComponent.NumericDic[NumericType.HP] = 0;
                numericComponent2.NumericDic[NumericType.MP] = 99;
                ret = BTHelper.RunTree(namedRoot, namedEnv);
                if (ret != ErrorCode.ERR_Success)
                {
                    Log.Console($"named owner second branch success error, expected: 0, actual: {ret}");
                    return 6;
                }
            }
            finally
            {
                namedEnv.Dispose();
            }

            return ErrorCode.ERR_Success;
        }
    }
}
