using ET.Server;

namespace ET.Test
{
    public class Test_RobotManagerCreateRobot_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestCase,
                nameof(Test_RobotManagerCreateRobot_Test));
            Fiber testFiber = scope.TestFiber;
            Scene root = testFiber.Root;

            root.AddComponent<ProcessInnerSender>();
            RobotManagerComponent robotManagerComponent = root.AddComponent<RobotManagerComponent>();
            long robotId = await robotManagerComponent.NewRobot(SchedulerType.Parent, nameof(Test_RobotManagerCreateRobot_Test));

            Fiber robotFiber = testFiber.GetFiber(robotId);
            if (robotFiber == null)
            {
                Log.Console($"robot fiber not found after RobotManagerComponent.NewRobot: {robotId}");
                return 1;
            }

            TestHelper.GetMap(testFiber, robotFiber);
            TestHelper.GetServerUnit(testFiber, robotFiber);

            return ErrorCode.ERR_Success;
        }
    }
}
