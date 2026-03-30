using System.Collections.Generic;
using ET.Server;
using SimpleJSON;

namespace ET.Test
{
    public static class FiberConfigSingletonMockFactory
    {
        public static UnitConfigCategory CreateUnitConfigCategory(int id, string name)
        {
            return new UnitConfigCategory(new Dictionary<int, UnitConfig>
            {
                [id] = new(id, UnitType.Player, name, EClassType.None, new Dictionary<int, long>())
            });
        }

        public static SpellConfigCategory CreateSpellConfigCategory(int id, int buffId, string desc)
        {
            SpellConfigCategory category = new();
            category.Add(new SpellConfig
            {
                Id = id,
                BuffId = buffId,
                Desc = desc,
            });
            category.ResolveRef();
            return category;
        }

        public static BuffConfigCategory CreateBuffConfigCategory(int id, string desc)
        {
            BuffConfigCategory category = new();
            category.Add(new BuffConfig
            {
                Id = id,
                Desc = desc,
            });
            category.ResolveRef();
            return category;
        }

        public static StartMachineConfigCategory CreateStartMachineConfigCategory(int id, string innerIp, string outerIp)
        {
            return new StartMachineConfigCategory(JSON.Parse($$"""
            [
              {
                "Id": {{id}},
                "InnerIP": "{{innerIp}}",
                "OuterIP": "{{outerIp}}"
              }
            ]
            """));
        }

        public static StartProcessConfigCategory CreateStartProcessConfigCategory(int id, int machineId, int port, string name)
        {
            return new StartProcessConfigCategory(JSON.Parse($$"""
            [
              {
                "Id": {{id}},
                "MachineId": {{machineId}},
                "Port": {{port}},
                "Num": 1,
                "Name": "{{name}}"
              }
            ]
            """));
        }

        public static StartSceneConfigCategory CreateStartSceneConfigCategory(int id, int processId, int zone, int port, string name, string sceneType)
        {
            return new StartSceneConfigCategory(JSON.Parse($$"""
            [
              {
                "Id": {{id}},
                "Process": {{processId}},
                "Zone": {{zone}},
                "SceneType": "{{sceneType}}",
                "Name": "{{name}}",
                "Port": {{port}}
              }
            ]
            """));
        }

        public static StartZoneConfigCategory CreateStartZoneConfigCategory(int id, int zoneType, string dbConnection, string dbName)
        {
            return new StartZoneConfigCategory(JSON.Parse($$"""
            [
              {
                "Id": {{id}},
                "ZoneType": {{zoneType}},
                "DBConnection": "{{dbConnection}}",
                "DBName": "{{dbName}}"
              }
            ]
            """));
        }
    }
}
