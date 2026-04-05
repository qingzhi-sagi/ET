using System;
using System.Collections.Generic;

namespace ET.Server
{
    public static class LocationPrimaryHelper
    {
        public static ulong GetPriorityId(ServiceInfo serviceInfo)
        {
            if (serviceInfo?.Metadata == null)
            {
                throw new RpcException(ErrorCode.ERR_LocationPrimaryUnavailable,
                    $"location service metadata missing scene: {serviceInfo?.SceneName}");
            }

            if (!serviceInfo.Metadata.TryGetValue(ServiceMetaKey.PriorityId, out string rawPriorityId)
                || string.IsNullOrWhiteSpace(rawPriorityId))
            {
                throw new RpcException(ErrorCode.ERR_LocationPrimaryUnavailable,
                    $"location service priority missing scene: {serviceInfo.SceneName}");
            }

            if (ulong.TryParse(rawPriorityId, out ulong priorityId))
            {
                return priorityId;
            }

            if (!long.TryParse(rawPriorityId, out long signedPriorityId))
            {
                throw new RpcException(ErrorCode.ERR_LocationPrimaryUnavailable,
                    $"location service priority invalid scene: {serviceInfo.SceneName} priority: {rawPriorityId}");
            }

            return unchecked((ulong)signedPriorityId);
        }

        public static ServiceInfo SelectStablePrimaryServiceInfo(List<ServiceInfo> serviceInfos)
        {
            ulong selectedPriorityId = ulong.MaxValue;
            ServiceInfo selectedServiceInfo = null;

            foreach (ServiceInfo serviceInfo in serviceInfos)
            {
                ulong priorityId = GetPriorityId(serviceInfo);

                if (selectedServiceInfo == null
                    || priorityId < selectedPriorityId
                    || (priorityId == selectedPriorityId && string.CompareOrdinal(serviceInfo.SceneName, selectedServiceInfo.SceneName) < 0))
                {
                    selectedPriorityId = priorityId;
                    selectedServiceInfo = serviceInfo;
                }
            }

            if (selectedServiceInfo == null)
            {
                throw new RpcException(ErrorCode.ERR_LocationPrimaryUnavailable, "not found location scene");
            }

            return selectedServiceInfo;
        }

        public static string SelectStablePrimarySceneName(List<ServiceInfo> serviceInfos)
        {
            return SelectStablePrimaryServiceInfo(serviceInfos).SceneName;
        }
    }
}
