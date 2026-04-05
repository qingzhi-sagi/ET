namespace ET.Server
{
    [MessageHandler(SceneType.ServiceDiscovery)]
    public class ServiceSubscribeRequestHandler : MessageHandler<Scene, ServiceSubscribeRequest, ServiceSubscribeResponse>
    {
        protected override async ETTask Run(Scene scene, ServiceSubscribeRequest request, ServiceSubscribeResponse response)
        {
            response.Error = ErrorCode.ERR_ServiceDiscoveryInvalidArgument;
            response.Message = "direct subscribe on ServiceDiscovery is not supported; use ServiceDiscoveryAgent";
            await ETTask.CompletedTask;
        }
    }
}
