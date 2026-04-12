using ET.Client;

namespace ET.Test
{
    public class Test_RouterManagerAddressIsolation_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scopeA = await TestFiberScope.Create(context.Fiber, SceneType.TestCase,
                $"{nameof(Test_RouterManagerAddressIsolation_Test)}_A");
            await using TestFiberScope scopeB = await TestFiberScope.Create(context.Fiber, SceneType.TestCase,
                $"{nameof(Test_RouterManagerAddressIsolation_Test)}_B");

            Fiber testFiberA = scopeA.TestFiber;
            Fiber testFiberB = scopeB.TestFiber;

            string addressA = TestHelper.GetRouterManagerAddress(testFiberA);
            string addressB = TestHelper.GetRouterManagerAddress(testFiberB);
            int portA = NetworkHelper.ToIPEndPoint(addressA).Port;
            int portB = NetworkHelper.ToIPEndPoint(addressB).Port;

            if (portA <= 0 || portB <= 0)
            {
                Log.Console($"router manager address invalid: A={addressA}, B={addressB}");
                return 1;
            }

            if (portA == portB)
            {
                Log.Console($"router manager address must be isolated: A={addressA}, B={addressB}");
                return 2;
            }

            Fiber robotA = await TestHelper.CreateRobot(testFiberA, $"{nameof(Test_RouterManagerAddressIsolation_Test)}_RobotA");
            Fiber robotB = await TestHelper.CreateRobot(testFiberB, $"{nameof(Test_RouterManagerAddressIsolation_Test)}_RobotB");

            TestHelper.GetMap(testFiberA, robotA);
            TestHelper.GetServerUnit(testFiberA, robotA);
            TestHelper.GetMap(testFiberB, robotB);
            TestHelper.GetServerUnit(testFiberB, robotB);

            return ErrorCode.ERR_Success;
        }
    }
}
