using System.Collections.Generic;

namespace ET.Test
{
    public class Test_ParallelRunner_Metadata_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            int configRet = AssertMode(nameof(Test_ConfigLoader_CodeConfigReload_Test), TestExecutionMode.Exclusive);
            if (configRet != 0)
            {
                return configRet;
            }

            int singletonRet = AssertMode(nameof(Core_Fiber_SingletonContract_Test), TestExecutionMode.Exclusive);
            if (singletonRet != 0)
            {
                return singletonRet;
            }

            int serviceDiscoveryRet = AssertMode(nameof(Servicediscovery_ConcurrentElection_Test), TestExecutionMode.Parallel);
            if (serviceDiscoveryRet != 0)
            {
                return serviceDiscoveryRet;
            }

            int pureRet = AssertMode(nameof(Core_Kcp_Config_Test), TestExecutionMode.Parallel);
            if (pureRet != 0)
            {
                return pureRet;
            }

            return ErrorCode.ERR_Success;
        }

        private static int AssertMode(string testName, TestExecutionMode expectedMode)
        {
            List<TestCaseInfo> testCases = TestDispatcher.Instance.Get(testName);
            if (testCases.Count != 1)
            {
                Log.Console($"metadata test expected one case: {testName}, actual: {testCases.Count}");
                return 1;
            }

            if (testCases[0].Mode != expectedMode)
            {
                Log.Console($"metadata test mode mismatch: {testName}, expected: {expectedMode}, actual: {testCases[0].Mode}");
                return 2;
            }

            return 0;
        }
    }
}
