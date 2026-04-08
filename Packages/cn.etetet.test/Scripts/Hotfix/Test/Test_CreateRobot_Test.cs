using System;
using System.Collections.Generic;
using ET.Client;
using ET.Server;
using MongoDB.Driver;

namespace ET.Test
{
    public class Test_CreateRobot_Test: ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestCase, nameof(Test_CreateRobot_Test));
            Fiber testFiber = scope.TestFiber;
            
            Fiber robot = await TestHelper.CreateRobot(testFiber, nameof(Test_CreateRobot_Test));
            
            Fiber map = TestHelper.GetMap(testFiber, robot);

            TestHelper.GetServerUnit(testFiber, robot);
            
            return ErrorCode.ERR_Success;
        }
    }

    public class Test_TestCaseFiberCleanup_DropsZoneDb_Test : ATestHandler
    {
        private const long DatabaseWaitTimeoutMs = 5000;
        private const long DatabaseStatePollIntervalMs = 50;

        public override async ETTask<int> Handle(TestContext context)
        {
            string zoneDbName;
            await using (TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestCase,
                             nameof(Test_TestCaseFiberCleanup_DropsZoneDb_Test)))
            {
                Fiber testFiber = scope.TestFiber;
                Scene root = testFiber.Root;
                EntityRef<Scene> rootRef = root;
                DBManagerComponent dbManagerComponent = root.GetComponent<DBManagerComponent>();
                zoneDbName = dbManagerComponent.GetZoneDBName(testFiber.Zone);

                Fiber robot = await TestHelper.CreateRobot(testFiber, nameof(Test_TestCaseFiberCleanup_DropsZoneDb_Test));
                root = rootRef;
                bool dbExists = await WaitForDatabaseStateAsync(root, zoneDbName, true, DatabaseWaitTimeoutMs);
                if (!dbExists)
                {
                    Log.Console($"cleanup test database not created before dispose: {zoneDbName}");
                    return 2;
                }
            }

            bool dbDeleted =
                    await WaitForDatabaseStateAsync(context.Fiber.Root, zoneDbName, false, DatabaseWaitTimeoutMs);
            if (!dbDeleted)
            {
                Log.Console($"cleanup test database still exists after dispose: {zoneDbName}");
                return 3;
            }

            return ErrorCode.ERR_Success;
        }

        [EnableGetComponent(typeof(TimerComponent))]
        [EnableGetComponent(typeof(DBManagerComponent))]
        private static async ETTask<bool> WaitForDatabaseStateAsync(Scene scene, string zoneDbName, bool shouldExist, long timeoutMs)
        {
            if (scene == null || string.IsNullOrWhiteSpace(zoneDbName))
            {
                return false;
            }

            TimerComponent timerComponent = scene.TimerComponent;
            if (timerComponent == null)
            {
                return false;
            }

            IMongoClient mongoClient = CreateMongoClientForTest();
            if (mongoClient == null)
            {
                return false;
            }

            EntityRef<TimerComponent> timerRef = timerComponent;
            long deadline = TimeInfo.Instance.ServerNow() + timeoutMs;
            while (TimeInfo.Instance.ServerNow() <= deadline)
            {
                bool exists = await ContainsDatabaseAsync(mongoClient, zoneDbName);
                if (exists == shouldExist)
                {
                    return true;
                }

                timerComponent = timerRef;
                if (timerComponent == null)
                {
                    return false;
                }

                await timerComponent.WaitAsync(DatabaseStatePollIntervalMs);
            }

            return false;
        }

        private static IMongoClient CreateMongoClientForTest()
        {
            StartZoneConfigCategory category = World.Instance.GetSingleton<StartZoneConfigCategory>();
            if (category == null)
            {
                return null;
            }

            StartZoneConfig config = category.GetOrDefault(0);
            if (config == null || string.IsNullOrEmpty(config.DBConnection))
            {
                foreach (StartZoneConfig item in category.GetAll().Values)
                {
                    if (item != null && !string.IsNullOrEmpty(item.DBConnection))
                    {
                        config = item;
                        break;
                    }
                }
            }

            if (config == null || string.IsNullOrEmpty(config.DBConnection))
            {
                return null;
            }

            return new MongoClient(config.DBConnection);
        }

        private static async ETTask<bool> ContainsDatabaseAsync(IMongoClient mongoClient, string zoneDbName)
        {
            using IAsyncCursor<string> cursor = await mongoClient.ListDatabaseNamesAsync();
            List<string> databaseNames = await cursor.ToListAsync();
            return databaseNames.Contains(zoneDbName);
        }
    }
}
