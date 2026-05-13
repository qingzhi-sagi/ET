namespace ET.Test
{
    public class Unitybridge_UnityTestRunReceivesArgs_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            if (context.Args == null)
            {
                Log.Console("UnityTestRun should execute through TestDispatcher and pass TestArgs");
                return 1;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(nameof(Unitybridge_UnityTestRunReceivesArgs_Test), context.Args.Name))
            {
                Log.Console($"UnityTestRun TestArgs name mismatch: {context.Args.Name}");
                return 2;
            }

            return ErrorCode.ERR_Success;
        }
    }
}
