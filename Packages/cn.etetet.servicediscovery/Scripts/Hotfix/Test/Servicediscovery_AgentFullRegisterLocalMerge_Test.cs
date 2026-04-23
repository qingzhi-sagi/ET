using System;
using System.Collections.Generic;
using System.Reflection;
using ET.Server;

namespace ET.Test
{
    [SkipAwaitEntityCheck]
    public class Servicediscovery_AgentFullRegisterLocalMerge_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            ServiceDiscoveryAgent agent = new();
            Address localAddress = new("127.0.0.1", 32000);
            Address remoteAddress = new(localAddress.IP, localAddress.Port + 1000);

            ActorId staleLocalActorId =
                new(localAddress, ServiceDiscovery_HA_TestHelper.CreateFiberInstanceId(context.Fiber.Zone, 850001));
            ActorId oldMatchActorId =
                new(localAddress, ServiceDiscovery_HA_TestHelper.CreateFiberInstanceId(context.Fiber.Zone, 850002));
            ActorId currentMatchActorId =
                new(localAddress, ServiceDiscovery_HA_TestHelper.CreateFiberInstanceId(context.Fiber.Zone, 850003));
            ActorId freshLocalActorId =
                new(localAddress, ServiceDiscovery_HA_TestHelper.CreateFiberInstanceId(context.Fiber.Zone, 850004));
            ActorId remoteActorId =
                new(remoteAddress, ServiceDiscovery_HA_TestHelper.CreateFiberInstanceId(context.Fiber.Zone, 850005));
            ActorId foreignLocalActorId =
                new(remoteAddress, ServiceDiscovery_HA_TestHelper.CreateFiberInstanceId(context.Fiber.Zone, 850006));

            Dictionary<string, (ActorId ActorId, StringKV Metadata)> snapshot = new()
            {
                ["Stale_Local"] = (staleLocalActorId, new StringKV { { "Role", "Stale" } }),
                ["Match_Local"] = (oldMatchActorId, new StringKV { { "Role", "Old" } }),
                ["Remote_Service"] = (remoteActorId, new StringKV { { "Role", "Remote" } }),
            };

            agent.LocalPublishedServices.Clear();
            agent.LocalPublishedServices["Match_Local"] = (currentMatchActorId, new StringKV { { "Role", "Current" } });
            agent.LocalPublishedServices["Fresh_Local"] = (freshLocalActorId, new StringKV { { "Role", "Fresh" } });
            agent.LocalPublishedServices["Foreign_Local"] = (foreignLocalActorId, new StringKV { { "Role", "Foreign" } });

            MethodInfo mergeMethod = typeof(ServiceDiscoveryAgentSystem).GetMethod(
                "MergeCurrentOwnedLocalServicesIntoSnapshot",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (mergeMethod == null)
            {
                Log.Console("agent full register local merge method missing");
                return 3;
            }

            mergeMethod.Invoke(null, new object[] { agent, localAddress, snapshot });

            if (snapshot.ContainsKey("Stale_Local"))
            {
                Log.Console("agent full register local merge kept stale local service");
                return 4;
            }

            if (!snapshot.TryGetValue("Match_Local", out (ActorId ActorId, StringKV Metadata) matchService) ||
                matchService.ActorId != currentMatchActorId ||
                !matchService.Metadata.TryGetValue("Role", out string matchRole) ||
                matchRole != "Current")
            {
                Log.Console("agent full register local merge did not prefer current local published service");
                return 5;
            }

            if (!snapshot.TryGetValue("Fresh_Local", out (ActorId ActorId, StringKV Metadata) freshService) ||
                freshService.ActorId != freshLocalActorId ||
                !freshService.Metadata.TryGetValue("Role", out string freshRole) ||
                freshRole != "Fresh")
            {
                Log.Console("agent full register local merge did not add fresh local published service");
                return 6;
            }

            if (!snapshot.TryGetValue("Remote_Service", out (ActorId ActorId, StringKV Metadata) remoteService) ||
                remoteService.ActorId != remoteActorId ||
                !remoteService.Metadata.TryGetValue("Role", out string remoteRole) ||
                remoteRole != "Remote")
            {
                Log.Console("agent full register local merge changed remote service");
                return 7;
            }

            if (snapshot.ContainsKey("Foreign_Local"))
            {
                Log.Console("agent full register local merge added foreign-address local service");
                return 8;
            }

            await ETTask.CompletedTask;

            Log.Console("ServiceDiscovery AgentFullRegisterLocalMerge passed");
            return ErrorCode.ERR_Success;
        }
    }
}
