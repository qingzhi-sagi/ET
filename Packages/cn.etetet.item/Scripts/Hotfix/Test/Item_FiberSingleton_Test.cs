using System;
using System.Collections.Generic;
using System.Linq;
using ET.Server;

namespace ET.Test
{
    public class Item_FiberSingleton_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestCase, nameof(Item_FiberSingleton_Test));
            Fiber testFiber = scope.TestFiber;

            Fiber robot = await TestHelper.CreateRobot(testFiber, "Client");
            Unit unit = TestHelper.GetServerUnit(testFiber, robot);
            ItemComponent itemComponent = unit.GetComponent<ItemComponent>();
            Fiber itemFiber = unit.Fiber();
            ItemConfig globalConfig = itemFiber.GetSingleton<ItemConfigCategory>().Get(10001);
            ItemConfigCategory mockCategory = new ItemConfigCategory(new Dictionary<int, ItemConfig>
            {
                [10001] = new ItemConfig(10001, "MockFiberItem", globalConfig.Type, 50, globalConfig.Quality, globalConfig.UseType, globalConfig.Level)
            });

            itemFiber.AddSingleton(mockCategory);
            try
            {
                ItemHelper.AddItem(itemComponent, 10001, 120, ItemChangeReason.QuestReward);

                int[] stackCounts = itemComponent.Children.Values
                        .OfType<Item>()
                        .Where(item => item.ConfigId == 10001)
                        .Select(item => item.Count)
                        .OrderBy(count => count)
                        .ToArray();

                if (itemComponent.GetUsedSlotCount() != 3 || !stackCounts.SequenceEqual(new[] { 20, 50, 50 }))
                {
                    throw new Exception($"fiber item singleton not used, stacks: [{string.Join(",", stackCounts)}]");
                }
            }
            finally
            {
                itemFiber.RemoveSingleton<ItemConfigCategory>();
            }

            ItemConfig fallbackConfig = itemFiber.GetSingleton<ItemConfigCategory>().Get(10001);
            if (!object.ReferenceEquals(fallbackConfig, globalConfig))
            {
                throw new Exception("item config singleton did not fall back to global after remove");
            }

            return ErrorCode.ERR_Success;
        }
    }
}

