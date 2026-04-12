using System;
using ET.Server;

namespace ET.Test
{
    public class Test_ConfigLoader_CodeConfigReload_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope =
                    await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty, nameof(Test_ConfigLoader_CodeConfigReload_Test));
            Fiber testFiber = scope.TestFiber;

            SpellConfigCategory beforeSpellCategory = testFiber.GetSingleton<SpellConfigCategory>();
            if (beforeSpellCategory == null)
            {
                return Fail(1, "spell config category should be initialized before reload");
            }

            SpellConfig beforeSpell = beforeSpellCategory.Get(100100);
            if (beforeSpell == null)
            {
                return Fail(2, "spell config 100100 should exist before reload");
            }

            BuffConfigCategory beforeBuffCategory = testFiber.GetSingleton<BuffConfigCategory>();
            if (beforeBuffCategory == null)
            {
                return Fail(3, "buff config category should be initialized before reload");
            }

            BuffConfig beforeBuff = beforeBuffCategory.Get(beforeSpell.BuffId);
            if (beforeBuff == null)
            {
                return Fail(4, "buff config referenced by spell should exist before reload");
            }

            StartSceneConfigCategory beforeStartSceneCategory = testFiber.GetSingleton<StartSceneConfigCategory>();
            if (beforeStartSceneCategory == null)
            {
                return Fail(5, "start scene config category should be initialized before reload");
            }

            int beforeSceneCount = beforeStartSceneCategory.GetByProcess(Options.Instance.Process).Count;
            if (beforeSceneCount == 0)
            {
                return Fail(6, "start scene config should contain current process scenes before reload");
            }

            await ConfigLoader.Instance.LoadAsync();

            SpellConfigCategory afterSpellCategory = testFiber.GetSingleton<SpellConfigCategory>();
            if (afterSpellCategory == null)
            {
                return Fail(7, "spell config category should still exist after reload");
            }

            if (ReferenceEquals(afterSpellCategory, beforeSpellCategory))
            {
                return Fail(8, "spell config category singleton should be replaced on reload");
            }

            SpellConfig afterSpell = testFiber.GetSingleton<SpellConfigCategory>().Get(beforeSpell.Id);
            if (afterSpell == null)
            {
                return Fail(9, "spell config should still exist after reload");
            }

            if (afterSpell.BuffId != beforeSpell.BuffId || afterSpell.Desc != beforeSpell.Desc || afterSpell.CD != beforeSpell.CD)
            {
                return Fail(10, "spell config data changed after reload");
            }

            BuffConfig afterBuff = testFiber.GetSingleton<BuffConfigCategory>().Get(afterSpell.BuffId);
            if (afterBuff == null)
            {
                return Fail(11, "buff config should still exist after reload");
            }

            if (afterBuff.Desc != beforeBuff.Desc || afterBuff.Duration != beforeBuff.Duration || afterBuff.TickTime != beforeBuff.TickTime)
            {
                return Fail(12, "buff config data changed after reload");
            }

            StartSceneConfigCategory afterStartSceneCategory = testFiber.GetSingleton<StartSceneConfigCategory>();
            if (afterStartSceneCategory == null)
            {
                return Fail(13, "start scene config category should still exist after reload");
            }

            if (ReferenceEquals(afterStartSceneCategory, beforeStartSceneCategory))
            {
                return Fail(14, "start scene config category singleton should be replaced on reload");
            }

            if (afterStartSceneCategory.GetByProcess(Options.Instance.Process).Count != beforeSceneCount)
            {
                return Fail(15, "start scene config count changed after reload");
            }

            return ErrorCode.ERR_Success;
        }

        private static int Fail(int code, string message)
        {
            Log.Console(message);
            return code;
        }
    }
}
