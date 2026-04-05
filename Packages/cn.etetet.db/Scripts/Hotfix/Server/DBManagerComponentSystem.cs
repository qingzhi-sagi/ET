using System;
using MongoDB.Driver;

namespace ET.Server
{
    public static partial class DBManagerComponentSystem
    {
        private const string ZoneDBNameSeparator = "_zone_";
        private const int ZoneDBNameWidth = 4;

        private static StartZoneConfig GetStartZoneConfig(this DBManagerComponent self, int zone)
        {
            StartZoneConfigCategory startZoneConfigCategory =
                    self.Root().Fiber().GetSingleton<StartZoneConfigCategory>()
                    ?? World.Instance.GetSingleton<StartZoneConfigCategory>();
            if (startZoneConfigCategory == null)
            {
                throw new Exception("start zone config category is not initialized");
            }

            StartZoneConfig startZoneConfig = startZoneConfigCategory.GetOrDefault(zone);
            if (startZoneConfig != null)
            {
                return startZoneConfig;
            }

            throw new Exception($"not found start zone config, zone: {zone}");
        }

        public static string GetZoneDBName(this DBManagerComponent self, int zone)
        {
            StartZoneConfigCategory startZoneConfigCategory =
                    self.Root().Fiber().GetSingleton<StartZoneConfigCategory>()
                    ?? World.Instance.GetSingleton<StartZoneConfigCategory>();
            if (startZoneConfigCategory == null)
            {
                throw new Exception("start zone config category is not initialized");
            }

            StartZoneConfig zoneConfig = startZoneConfigCategory.GetOrDefault(zone);
            if (zoneConfig == null)
            {
                throw new Exception($"not found start zone config, zone: {zone}");
            }

            if (string.IsNullOrEmpty(zoneConfig.DBName))
            {
                return string.Empty;
            }

            string formattedZone = zone.ToString($"D{ZoneDBNameWidth}");
            return $"{zoneConfig.DBName}{ZoneDBNameSeparator}{formattedZone}";
        }

        public static async ETTask DropDB(this DBManagerComponent self, int zone)
        {
            StartZoneConfig startZoneConfig = self.GetStartZoneConfig(zone);
            string dbName = self.GetZoneDBName(zone);
            if (string.IsNullOrEmpty(startZoneConfig.DBConnection) || string.IsNullOrEmpty(dbName))
            {
                return;
            }

            MongoClient mongoClient = new(startZoneConfig.DBConnection);
            await mongoClient.DropDatabaseAsync(dbName);
            await ETTask.CompletedTask;
        }
        
        public static DBComponent GetZoneDB(this DBManagerComponent self, int zone)
        {
            DBComponent dbComponent = self.GetChild<DBComponent>(zone);
            if (dbComponent != null)
            {
                return dbComponent;
            }

            StartZoneConfig startZoneConfig = self.GetStartZoneConfig(zone);
            if (string.IsNullOrEmpty(startZoneConfig.DBConnection))
            {
                throw new Exception($"zone: {zone} not found mongo connect string");
            }

            string dbName = self.GetZoneDBName(zone);
            if (string.IsNullOrEmpty(dbName))
            {
                throw new Exception($"zone: {zone} not found mongo db name");
            }

            dbComponent = self.AddChildWithId<DBComponent, string, string>(zone, startZoneConfig.DBConnection, dbName);
            return dbComponent;
        }
    }
}
