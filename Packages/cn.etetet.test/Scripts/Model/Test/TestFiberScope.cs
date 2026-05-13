using System;
using System.Threading.Tasks;

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
            scope.TestFiber = await fiber.CreateFiber(zone, IdGenerater.Instance.GenerateId(), sceneType, testName);
            if (sceneType == SceneType.TestEmpty)
            {
                scope.TestFiber.AddSingleton<ProcessFiberAddressSingleton>();
            }
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
            return zone;
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
