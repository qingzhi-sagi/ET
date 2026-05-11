using System;

namespace ET
{
    internal sealed class UnityBridgePingHandler : AUnityBridgeHandler<Ping, PingResponse>
    {
        protected override async ETTask<IResponse> Run(Ping command)
        {
            await ETTask.CompletedTask;
            PingResponse response = PingResponse.Create();
            response.Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return response;
        }
    }
}
