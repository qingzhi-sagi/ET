namespace ET.Test
{
    public class Unitybridge_SharedTestBase_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            string testHandlerAssembly = typeof(ATestHandler).Assembly.GetName().Name;
            if (testHandlerAssembly == "ET.Editor")
            {
                Log.Console("ATestHandler should come from the shared test framework assembly");
                return 1;
            }

            string testContextAssembly = typeof(TestContext).Assembly.GetName().Name;
            if (testContextAssembly == "ET.Editor")
            {
                Log.Console("TestContext should come from the shared test framework assembly");
                return 2;
            }

            return ErrorCode.ERR_Success;
        }
    }
}
