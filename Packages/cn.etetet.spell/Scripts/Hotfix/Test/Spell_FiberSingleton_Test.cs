using System;
using ET.Server;

namespace ET.Test
{
    public class Spell_FiberSingleton_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, nameof(Spell_FiberSingleton_Test));
            Fiber testFiber = scope.TestFiber;

            Fiber robot = await TestHelper.CreateRobot(testFiber, "Client");
            Unit unit = TestHelper.GetServerUnit(testFiber, robot);
            Fiber unitFiber = unit.Fiber();

            SpellConfig globalSpellConfig = unitFiber.GetSingleton<SpellConfigCategory>().Get(100100);
            BuffConfig globalBuffConfig = unitFiber.GetSingleton<BuffConfigCategory>().Get(globalSpellConfig.BuffId);

            SpellConfig mockSpellConfig = new SpellConfig()
            {
                Id = globalSpellConfig.Id,
                BuffId = globalSpellConfig.BuffId,
                Desc = "MockFiberSpell",
                CD = globalSpellConfig.CD + 1234,
            };
            BuffConfig mockBuffConfig = new BuffConfig()
            {
                Id = globalBuffConfig.Id,
                Desc = "MockFiberBuff",
                Duration = globalBuffConfig.Duration + 1234,
                TickTime = globalBuffConfig.TickTime,
                MaxStack = globalBuffConfig.MaxStack + 1,
                Stack = globalBuffConfig.Stack + 1,
                OverLayRuleType = globalBuffConfig.OverLayRuleType,
                NoticeType = globalBuffConfig.NoticeType,
            };
            mockBuffConfig.OnAfterDeserialize();

            SpellConfigCategory mockSpellCategory = new SpellConfigCategory();
            mockSpellCategory.Add(mockSpellConfig);
            BuffConfigCategory mockBuffCategory = new BuffConfigCategory();
            mockBuffCategory.Add(mockBuffConfig);
            mockBuffCategory.ResolveRef();

            unitFiber.AddSingleton(mockSpellCategory);
            unitFiber.AddSingleton(mockBuffCategory);
            try
            {
                BuffComponent buffComponent = unit.GetComponent<BuffComponent>();
                Buff buff = buffComponent.CreateBuff(IdGenerater.Instance.GenerateId(), mockBuffConfig.Id, unit.Id);
                try
                {
                    if (!object.ReferenceEquals(buff.GetConfig(), mockBuffConfig))
                    {
                        throw new Exception("buff config did not resolve from fiber singleton");
                    }

                    if (!object.ReferenceEquals(buff.GetSpellConfig(), mockSpellConfig))
                    {
                        throw new Exception("spell config did not resolve from fiber singleton");
                    }

                    if (buff.Stack != mockBuffConfig.Stack)
                    {
                        throw new Exception($"buff stack mismatch after fiber singleton create: {buff.Stack} != {mockBuffConfig.Stack}");
                    }
                }
                finally
                {
                    buffComponent.RemoveBuff(buff);
                }
            }
            finally
            {
                unitFiber.RemoveSingleton<SpellConfigCategory>();
                unitFiber.RemoveSingleton<BuffConfigCategory>();
            }

            if (!object.ReferenceEquals(unitFiber.GetSingleton<SpellConfigCategory>().Get(globalSpellConfig.Id), globalSpellConfig))
            {
                throw new Exception("spell config singleton did not fall back to global after remove");
            }

            if (!object.ReferenceEquals(unitFiber.GetSingleton<BuffConfigCategory>().Get(globalBuffConfig.Id), globalBuffConfig))
            {
                throw new Exception("buff config singleton did not fall back to global after remove");
            }

            return ErrorCode.ERR_Success;
        }
    }
}




