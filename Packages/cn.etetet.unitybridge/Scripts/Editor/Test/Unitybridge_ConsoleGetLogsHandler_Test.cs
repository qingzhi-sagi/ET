using System;
using System.Linq;
using UnityEngine;

namespace ET.Test
{
    public class Unitybridge_ConsoleGetLogsHandler_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            string marker = "Unitybridge_ConsoleGetLogsHandler_Test_" + Guid.NewGuid().ToString("N");
            Debug.Log(marker);

            ConsoleGetLogsRequest request = ConsoleGetLogsRequest.Create();
            request.Count = 100;
            request.LogType = "Log";

            IResponse rawResponse = await new UnityBridgeConsoleGetLogsHandler().Handle(request);
            if (rawResponse is not ConsoleGetLogsResponse response)
            {
                return UnityBridgeProtocolTestSupport.Fail(1, "ConsoleGetLogsHandler should return ConsoleGetLogsResponse");
            }

            if (response.Error != 0 || response.LogType != "Log")
            {
                return UnityBridgeProtocolTestSupport.Fail(2, "ConsoleGetLogsHandler should return successful log response");
            }

            if (!response.Logs.Any(log => log.Message.Contains(marker)))
            {
                return UnityBridgeProtocolTestSupport.Fail(3, "ConsoleGetLogsHandler should include recent Unity log entry");
            }

            return ErrorCode.ERR_Success;
        }
    }
}
