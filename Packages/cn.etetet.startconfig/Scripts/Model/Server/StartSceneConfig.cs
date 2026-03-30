using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ET.Server
{
    [EnableClass]
    public partial class StartSceneConfigCategory
    {
    }

    [EnableClass]
    public partial class StartZoneConfigCategory
    {
    }

    public partial class StartSceneConfigCategory
    {
        private readonly MultiMap<int, StartSceneConfig> processScenes = new();
        
        private readonly Dictionary<string, StartSceneConfig> sceneNameScenes = new();
        
        public List<StartSceneConfig> GetByProcess(int process)
        {
            return this.processScenes[process];
        }
        
        public StartSceneConfig GetBySceneName(string sceneName)
        {
            return this.sceneNameScenes[sceneName];
        }

        public List<StartSceneConfig> GetBySceneType(int zone, int sceneType)
        {
            string sceneTypeName = SceneTypeSingleton.Instance.GetSceneName(sceneType);
            return this.GetAll().Values.Where(config => config.Zone == zone && config.SceneType == sceneTypeName).ToList();
        }

        public StartSceneConfig GetOneBySceneType(int zone, int sceneType)
        {
            List<StartSceneConfig> configs = this.GetBySceneType(zone, sceneType);
            if (configs.Count == 1)
            {
                return configs[0];
            }

            throw new Exception($"scene config count is not one, zone: {zone}, sceneType: {sceneType}, count: {configs.Count}");
        }
        
        public override void EndInit()
        {
            foreach (StartSceneConfig startSceneConfig in this.GetAll().Values)
            {
                this.processScenes.Add(startSceneConfig.Process, startSceneConfig);
                this.sceneNameScenes.Add(startSceneConfig.Name, startSceneConfig);
            }
        }
    }
    
    public partial class StartSceneConfig
    {
        public override void EndInit()
        {
        }
    }

    public static class StartSceneConfigExtensions
    {
        public static StartProcessConfig GetStartProcessConfig(this StartSceneConfig self, Fiber fiber)
        {
            return fiber.GetSingleton<StartProcessConfigCategory>().Get(self.Process);
        }

        public static StartZoneConfig GetStartZoneConfig(this StartSceneConfig self, Fiber fiber)
        {
            return fiber.GetSingleton<StartZoneConfigCategory>().Get(self.Zone);
        }

        public static Address GetAddress(this StartSceneConfig self, Fiber fiber)
        {
            return self.GetStartProcessConfig(fiber).GetAddress(fiber);
        }

        public static IPEndPoint GetInnerIPOuterPort(this StartSceneConfig self, Fiber fiber)
        {
            return NetworkHelper.ToIPEndPoint($"{self.GetStartProcessConfig(fiber).GetInnerIP(fiber)}:{self.Port}");
        }

        public static IPEndPoint GetOuterIPOuterPort(this StartSceneConfig self, Fiber fiber)
        {
            return NetworkHelper.ToIPEndPoint($"{self.GetStartProcessConfig(fiber).GetOuterIP(fiber)}:{self.Port}");
        }

        public static ActorId GetActorId(this StartSceneConfig self, Fiber fiber)
        {
            return new ActorId(self.GetAddress(fiber), new FiberInstanceId(self.Id, 1));
        }
    }
}
