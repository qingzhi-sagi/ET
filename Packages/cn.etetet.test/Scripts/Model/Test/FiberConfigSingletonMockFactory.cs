using System.Collections.Generic;
using ET.Server;

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
            return new StartMachineConfigCategory(new Dictionary<int, StartMachineConfig>
            {
                [id] = new StartMachineConfig(id, innerIp, outerIp),
            });
        }

        public static StartProcessConfigCategory CreateStartProcessConfigCategory(int id, int machineId, int port, string name)
        {
            return new StartProcessConfigCategory(new Dictionary<int, StartProcessConfig>
            {
                [id] = new StartProcessConfig(id, machineId, port, 1, name),
            });
        }

        public static StartSceneConfigCategory CreateStartSceneConfigCategory(int id, int processId, int zone, int port, string name, string sceneType)
        {
            return new StartSceneConfigCategory(new Dictionary<int, StartSceneConfig>
            {
                [id] = new StartSceneConfig(id, processId, zone, sceneType, name, port),
            });
        }

        public static StartZoneConfigCategory CreateStartZoneConfigCategory(int id, int zoneType, string dbConnection, string dbName)
        {
            return new StartZoneConfigCategory(new Dictionary<int, StartZoneConfig>
            {
                [id] = new StartZoneConfig(id, zoneType, dbConnection, dbName),
            });
        }
    }
}
