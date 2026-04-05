namespace ET.Server
{
    [MessageHandler(SceneType.ServiceDiscovery)]
    public class ServiceQueryRequestHandler : MessageHandler<Scene, ServiceQueryRequest, ServiceQueryResponse>
    {
        protected override async ETTask Run(Scene scene, ServiceQueryRequest request, ServiceQueryResponse response)
        {
            response.Error = ErrorCode.ERR_ServiceDiscoveryInvalidArgument;
            response.Message = "direct query on ServiceDiscovery is not supported; use ServiceDiscoveryAgent";
            await ETTask.CompletedTask;
        }
    }
}
