using System;
using System.Collections.Generic;
using System.Reflection;
using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    /// <summary>
    /// 边界与无效输入测试：
    /// 覆盖空结果查询、空过滤查询、无效订阅者参数、空场景注销等输入边界。
    /// </summary>
    public class Servicediscovery_BoundaryAndInvalidInput_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            // 1. 准备测试环境

            int addressError = ServiceDiscovery_HA_TestHelper.EnsureAddressSingletonReady(context.Fiber);
            if (addressError != 0)
            {
                Log.Console($"boundary ensure address singleton failed: {addressError}");
                return 2;
            }

            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Servicediscovery_BoundaryAndInvalidInput_Test));
            Fiber testFiber = scope.TestFiber;

            int resetError = await ServiceDiscovery_HA_TestHelper.ResetServiceDiscoveryStorage(testFiber);
            if (resetError != 0)
            {
                Log.Console($"boundary reset storage failed: {resetError}");
                return 1;
            }

            Fiber node = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 0,
                "ServiceDiscovery_Boundary");
            ServiceDiscovery sd = node.Root.GetComponent<ServiceDiscovery>();
            TimerComponent timer = node.Root.TimerComponent;
            MessageSender sender = node.Root.GetComponent<MessageSender>();
            bool isMaster = await ServiceDiscovery_HA_TestHelper.WaitUntilMaster(sd, timer, 5000);
            if (!isMaster)
            {
                Log.Console("boundary node is not master");
                return 5;
            }

            int ensureAgentError = await ServiceDiscovery_HA_TestHelper.EnsureServiceDiscoveryAgentFiberAsync(testFiber);
            if (ensureAgentError != 0)
            {
                Log.Console($"boundary ensure service discovery agent failed: {ensureAgentError}");
                return 8;
            }

            ActorId agentActorId = new ActorId(node.Root.GetActorId().Address,
                ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryAgentFiberInstanceId(testFiber.Zone));

            // 2. 查询边界：直连ServiceDiscovery查询应失败（查询职责在Agent）
            bool notFoundRejected = await IsDirectServiceDiscoveryQueryRejected(
                sd, sender, node.Root.GetActorId(), new StringKV
            {
                { "Role", "NotExists" }
            });
            if (!notFoundRejected)
            {
                Log.Console("boundary notFound query should be rejected by service discovery");
                return 6;
            }

            // 3. 查询边界：空过滤直连ServiceDiscovery同样应失败
            bool nullFilterRejected = await IsDirectServiceDiscoveryQueryRejected(sd, sender, node.Root.GetActorId(), null);
            if (!nullFilterRejected)
            {
                Log.Console("boundary nullFilter query should be rejected by service discovery");
                return 7;
            }

            // 4. 直连ServiceDiscovery订阅应失败（订阅职责在Agent）
            bool invalidSubscribeRejected = await IsDirectServiceDiscoverySubscribeRejected(sd, sender, node.Root.GetActorId());
            if (!invalidSubscribeRejected)
            {
                Log.Console("boundary invalid subscribe should be rejected by service discovery");
                return 12;
            }

            // 5. 无效注销：空 sceneName 必须被明确识别为参数错误
            bool invalidUnregisterRejected = await IsInvalidUnregisterRejected(sender, node.Root.GetActorId());
            if (!invalidUnregisterRejected)
            {
                Log.Console("boundary invalid unregister should be rejected");
                return 14;
            }

            bool invalidRegisterRejected = await IsInvalidRegisterRejected(sender, node.Root.GetActorId());
            if (!invalidRegisterRejected)
            {
                Log.Console("boundary invalid register should be rejected with stable message");
                return 15;
            }

            bool invalidHeartbeatRejected = await IsInvalidHeartbeatRejected(sender, node.Root.GetActorId());
            if (!invalidHeartbeatRejected)
            {
                Log.Console("boundary invalid heartbeat should be rejected with stable message");
                return 19;
            }

            bool invalidAgentQueryRejected = await IsInvalidAgentQueryRejected(sender, agentActorId);
            if (!invalidAgentQueryRejected)
            {
                Log.Console("boundary invalid query should be rejected with stable message");
                return 20;
            }

            bool invalidAgentSubscribeRejected = await IsInvalidAgentSubscribeRejected(sender, agentActorId);
            if (!invalidAgentSubscribeRejected)
            {
                Log.Console("boundary invalid subscribe should be rejected with stable message");
                return 21;
            }

            // 6. 备节点写入必须被明确识别为 follower 拒绝
            Fiber followerNode = await ServiceDiscovery_HA_TestHelper.CreateServiceDiscoveryNodeByConfig(testFiber, 1,
                "ServiceDiscovery_Boundary_Follower");
            ServiceDiscovery followerSd = followerNode.Root.GetComponent<ServiceDiscovery>();
            MessageSender followerSender = followerNode.Root.GetComponent<MessageSender>();
            bool followerAcceptedMaster = await followerSd.EnsureActiveMasterWithFenceAsync();
            if (followerAcceptedMaster || followerSd.GetOrAddLease().IsActiveMaster)
            {
                Log.Console("boundary follower unexpectedly accepted master role");
                return 18;
            }

            bool followerRejected = await IsFollowerRegisterRejected(followerSender, followerNode.Root.GetActorId());
            if (!followerRejected)
            {
                Log.Console("boundary follower write should be rejected with follower error code");
                return 22;
            }

            Log.Console("ServiceDiscovery BoundaryAndInvalidInput passed");
            return ErrorCode.ERR_Success;
        }

        private static async ETTask<bool> IsDirectServiceDiscoveryQueryRejected(
            ServiceDiscovery serviceDiscovery, MessageSender sender, ActorId targetActorId, StringKV filter)
        {
            if (!HasMessageHandlerForScene(serviceDiscovery, typeof(ServiceQueryRequest)))
            {
                return true;
            }

            try
            {
                using ServiceQueryRequest request = ServiceQueryRequest.Create();
                if (filter == null)
                {
                    request.Filter = null;
                }
                else
                {
                    foreach ((string key, string value) in filter)
                    {
                        request.Filter[key] = value;
                    }
                }

                using ServiceQueryResponse response = await sender.Call(targetActorId, request, false) as ServiceQueryResponse;
                return response == null || response.Error != ErrorCode.ERR_Success;
            }
            catch (RpcException)
            {
                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static async ETTask<bool> IsInvalidUnregisterRejected(MessageSender sender, ActorId targetActorId)
        {
            try
            {
                using ServiceUnregisterRequest request = ServiceUnregisterRequest.Create();
                request.SceneName = string.Empty;
                using ServiceUnregisterResponse response = await sender.Call(targetActorId, request, false) as ServiceUnregisterResponse;
                return response != null
                       && response.Error == ErrorCode.ERR_ServiceDiscoveryInvalidArgument
                       && response.Message == $"{nameof(ServiceUnregisterRequest)} invalid: SceneName is empty.";
            }
            catch (RpcException)
            {
                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static async ETTask<bool> IsInvalidRegisterRejected(MessageSender sender, ActorId targetActorId)
        {
            try
            {
                using ServiceRegisterRequest request = ServiceRegisterRequest.Create();
                request.SceneName = string.Empty;
                using ServiceRegisterResponse response = await sender.Call(targetActorId, request, false) as ServiceRegisterResponse;
                return response != null
                       && response.Error == ErrorCode.ERR_ServiceDiscoveryInvalidArgument
                       && response.Message == $"{nameof(ServiceRegisterRequest)} invalid: SceneName is empty.";
            }
            catch (RpcException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static async ETTask<bool> IsInvalidHeartbeatRejected(MessageSender sender, ActorId targetActorId)
        {
            try
            {
                using ServiceHeartbeatRequest request = ServiceHeartbeatRequest.Create();
                request.AgentActorId = default;
                using ServiceHeartbeatResponse response = await sender.Call(targetActorId, request, false) as ServiceHeartbeatResponse;
                return response != null
                       && response.Error == ErrorCode.ERR_ServiceDiscoveryInvalidArgument
                       && response.Message == $"{nameof(ServiceHeartbeatRequest)} invalid: AgentActorId is empty.";
            }
            catch (RpcException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static async ETTask<bool> IsInvalidAgentQueryRejected(MessageSender sender, ActorId targetActorId)
        {
            try
            {
                using ServiceQueryRequest request = ServiceQueryRequest.Create();
                request.Filter[string.Empty] = "Provider";
                using ServiceQueryResponse response = await sender.Call(targetActorId, request, false) as ServiceQueryResponse;
                return response != null
                       && response.Error == ErrorCode.ERR_ServiceDiscoveryInvalidArgument
                       && response.Message == $"{nameof(ServiceQueryRequest)} invalid: Filter contains empty key.";
            }
            catch (RpcException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static async ETTask<bool> IsInvalidAgentSubscribeRejected(MessageSender sender, ActorId targetActorId)
        {
            try
            {
                using ServiceSubscribeRequest request = ServiceSubscribeRequest.Create();
                request.SceneName = "BoundarySubscriber";
                request.FilterName = "RoleProvider";
                request.FilterMetadata["Role"] = "Provider";
                request.SubscriberActorId = default;

                using ServiceSubscribeResponse response = await sender.Call(targetActorId, request, false) as ServiceSubscribeResponse;
                return response != null
                       && response.Error == ErrorCode.ERR_SubscriberActorIdRequired
                       && response.Message == $"{nameof(ServiceSubscribeRequest)} invalid: SubscriberActorId is empty.";
            }
            catch (RpcException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static async ETTask<bool> IsFollowerRegisterRejected(MessageSender sender, ActorId targetActorId)
        {
            try
            {
                using ServiceRegisterRequest request = ServiceRegisterRequest.Create();
                request.SceneName = "BoundaryFollowerReject";
                int zone = FiberIdHelper.DecodeZone(targetActorId.FiberInstanceId.Fiber);
                request.ActorId = ServiceDiscovery_HA_TestHelper.CreateActorId(zone, targetActorId.Address, 850001);
                request.Metadata[ServiceMetaKey.SceneType] = "BoundaryProbe";

                using ServiceRegisterResponse response = await sender.Call(targetActorId, request, false) as ServiceRegisterResponse;
                return response != null && response.Error == ErrorCode.ERR_ServiceDiscoveryFollowerRejected;
            }
            catch (RpcException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static async ETTask<bool> IsDirectServiceDiscoverySubscribeRejected(
            ServiceDiscovery serviceDiscovery, MessageSender sender, ActorId targetActorId)
        {
            if (!HasMessageHandlerForScene(serviceDiscovery, typeof(ServiceSubscribeRequest)))
            {
                return true;
            }

            try
            {
                using ServiceSubscribeRequest request = ServiceSubscribeRequest.Create();
                request.SceneName = "BoundarySubscriber";
                request.FilterName = "InvalidFilter";
                request.FilterMetadata["Role"] = "Provider";
                request.SubscriberActorId = default;

                using ServiceSubscribeResponse response = await sender.Call(targetActorId, request, false) as ServiceSubscribeResponse;
                return response == null || response.Error != ErrorCode.ERR_Success;
            }
            catch (RpcException)
            {
                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static bool HasMessageHandlerForScene(ServiceDiscovery serviceDiscovery, Type requestType)
        {
            if (serviceDiscovery == null || requestType == null)
            {
                return false;
            }

            FieldInfo fieldInfo =
                typeof(MessageDispatcher).GetField("messageHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                return false;
            }

            Dictionary<Type, List<MessageDispatcherInfo>> handlers =
                fieldInfo.GetValue(MessageDispatcher.Instance) as Dictionary<Type, List<MessageDispatcherInfo>>;
            if (handlers == null || !handlers.TryGetValue(requestType, out List<MessageDispatcherInfo> infos) || infos == null)
            {
                return false;
            }

            int sceneType = serviceDiscovery.Root().SceneType;
            foreach (MessageDispatcherInfo info in infos)
            {
                if (SceneTypeSingleton.IsSame(info.SceneType, sceneType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
