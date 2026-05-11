using System;

namespace ET
{
    public sealed class UnityBridgeTestEchoHandler : AUnityBridgeHandler<TestEcho, TestEchoResponse>
    {
        protected override async ETTask<IResponse> Run(TestEcho command)
        {
            await ETTask.CompletedTask;
            TestEchoResponse response = TestEchoResponse.Create();
            response.Text = command.Text;
            response.HandledAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            response.Handler = nameof(UnityBridgeTestEchoHandler);
            return response;
        }
    }
}
