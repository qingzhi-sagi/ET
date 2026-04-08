using System;
using System.Threading.Tasks;
using ET.Server;

namespace ET.Test
{
    public struct TestFiberScope : IAsyncDisposable
    {
        public Fiber TestFiber { get; private set; }
        private readonly Fiber fiber;

        public TestFiberScope(Fiber fiber)
        {
            this.fiber = fiber;
            this.TestFiber = null;
        }

        public static async ETTask<TestFiberScope> Create(Fiber fiber, int sceneType, string testName)
        {
            TestFiberScope scope = new(fiber);
            int zone = AllocateZone(fiber);
            scope.TestFiber = await fiber.CreateZoneFiber(zone, IdGenerater.Instance.GenerateId(), sceneType, testName);
            return scope;
        }

        public async ValueTask DisposeAsync()
        {
            if (this.fiber == null || this.TestFiber == null)
            {
                return;
            }

            // 注意这里因为是在ValueTask里面，ValueTask不带上下文，所以必须设置上下文，否则会卡住await无法回调回来
            await fiber.RemoveFiber(this.TestFiber.Id).NewContext(null);
        }

        private static int AllocateZone(Fiber fiber)
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
                foreach (StartZoneConfig item in category.GetAll().Values)
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

            StartZoneConfig generated = new(zone, template.ZoneType, template.DBConnection, template.DBName);

            category.GetAll().Add(zone, generated);
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
