using System;
using System.Threading.Tasks;
using ET.Server;
using SimpleJSON;

namespace ET.Test
{
    public struct TestFiberScope(Fiber fiber) : IAsyncDisposable
    {
        public Fiber TestFiber { get; private set; }

        public static async ETTask<TestFiberScope> Create(Fiber fiber, string testName)
        {
            return await Create(fiber, SceneType.TestCase, testName);
        }

        public static async ETTask<TestFiberScope> Create(Fiber fiber, int sceneType, string testName)
        {
            TestFiberScope scope = new(fiber);
            int zone = AllocateZone(fiber);
            scope.TestFiber = await fiber.CreateZoneFiber(zone, IdGenerater.Instance.GenerateId(), sceneType, testName);
            return scope;
        }

        public static async ETTask<TestFiberScope> CreateOneFiber(Fiber fiber, int sceneType, string testName)
        {
            TestFiberScope scope = new(fiber);
            int zone = AllocateZone(fiber);
            scope.TestFiber = await fiber.CreateZoneFiber(zone, IdGenerater.Instance.GenerateId(), sceneType, testName);
            return scope;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (fiber == null || this.TestFiber == null)
                {
                    return;
                }

                await fiber.RemoveFiber(this.TestFiber.Id).NewContext(null);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static int AllocateZone(Fiber fiber)
        {
            TestZoneAllocatorComponent allocator = GetAllocator(fiber);
            int zone = allocator.Zone;
            if (zone > FiberIdHelper.MaxZone)
            {
                throw new Exception($"test zone exhausted: {zone}");
            }

            allocator.Zone = zone + 1;
            EnsureStartZoneConfigExists(fiber, zone);
            return zone;
        }

        private static void EnsureStartZoneConfigExists(Fiber fiber, int zone)
        {
            StartZoneConfigCategory category = fiber?.GetSingleton<StartZoneConfigCategory>()
                    ?? World.Instance.GetSingleton<StartZoneConfigCategory>();
            if (category == null)
            {
                throw new Exception("StartZoneConfigCategory is not initialized");
            }

            if (category.GetOrDefault(zone) != null)
            {
                return;
            }

            StartZoneConfig template = category.GetOrDefault(0);
            if (template == null)
            {
                foreach (StartZoneConfig item in category.DataList)
                {
                    if (item != null && !string.IsNullOrEmpty(item.DBConnection))
                    {
                        template = item;
                        break;
                    }
                }
            }

            if (template == null)
            {
                throw new Exception($"cannot synthesize start zone config for test zone: {zone}");
            }

            StartZoneConfig generated = new(JSON.Parse($$"""
            {
              "Id": {{zone}},
              "ZoneType": {{template.ZoneType}},
              "DBConnection": "{{template.DBConnection}}",
              "DBName": "{{template.DBName}}"
            }
            """));

            category.DataMap.Add(zone, generated);
            category.DataList.Add(generated);
        }

        private static TestZoneAllocatorComponent GetAllocator(Fiber fiber)
        {
            TestZoneAllocatorComponent allocator = fiber?.Root?.GetComponent<TestZoneAllocatorComponent>();
            if (allocator == null)
            {
                throw new Exception("TestZoneAllocatorComponent is not initialized on test fiber before running tests");
            }

            return allocator;
        }
    }
}
