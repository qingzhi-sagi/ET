namespace ET.Server
{
    public static class ServiceDiscoveryErrorHelper
    {
        public static void SetInvalidArgument(IResponse response, string message)
        {
            response.Error = ErrorCode.ERR_ServiceDiscoveryInvalidArgument;
            response.Message = message;
        }

        public static void SetPersistenceFailed(IResponse response, string message)
        {
            response.Error = ErrorCode.ERR_ServiceDiscoveryPersistenceFailed;
            response.Message = message;
        }

        public static void SetInternalFailure(IResponse response, string message)
        {
            response.Error = ErrorCode.ERR_ServiceDiscoveryOperationFailed;
            response.Message = message;
        }

        public static void SetMasterUnavailable(IResponse response, string message)
        {
            response.Error = ErrorCode.ERR_ServiceDiscoveryMasterUnavailable;
            response.Message = message;
        }

        public static void SetFollowerRejected(IResponse response, ServiceDiscovery serviceDiscovery)
        {
            string currentMaster = serviceDiscovery?.GetOrAddLease().CurrentMasterSceneName;
            response.Error = ErrorCode.ERR_ServiceDiscoveryFollowerRejected;
            response.Message = $"service discovery is follower currentMaster: {currentMaster}";
        }

        public static void SetNotWritableMaster(IResponse response, ServiceDiscovery serviceDiscovery)
        {
            if (serviceDiscovery == null)
            {
                SetMasterUnavailable(response, "service discovery master unavailable");
                return;
            }

            if (!string.IsNullOrEmpty(serviceDiscovery.GetOrAddLease().CurrentMasterSceneName)
                || serviceDiscovery.GetOrAddLease().CurrentMasterActorId != default)
            {
                SetFollowerRejected(response, serviceDiscovery);
                return;
            }

            SetMasterUnavailable(response, "service discovery master unavailable");
        }

        public static bool ShouldTriggerFailover(int error)
        {
            return error == ErrorCode.ERR_ServiceDiscoveryFollowerRejected
                   || error == ErrorCode.ERR_ServiceDiscoveryMasterUnavailable
                   || error == ErrorCode.ERR_NotFoundActor
                   || error == ErrorCode.ERR_RpcFail
                   || error == ErrorCode.ERR_MessageTimeout;
        }
    }
}
