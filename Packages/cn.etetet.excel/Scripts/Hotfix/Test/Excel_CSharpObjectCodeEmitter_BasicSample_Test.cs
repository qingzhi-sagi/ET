using System;

namespace ET.Test
{
    public class Excel_CSharpObjectCodeEmitter_BasicSample_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            CSharpObjectCodeEmitter emitter = new();
            CSharpObjectCodeEmitterBasicSample sample = new()
            {
                Id = 1001,
                Name = "EmitterBasic",
                Child = new CSharpObjectCodeEmitterConcreteChild
                {
                    ChildId = 2,
                    Label = "Child_A",
                },
            };

            string code = emitter.Emit(sample);

            AssertContains(code, "new global::ET.Test.CSharpObjectCodeEmitterBasicSample");
            AssertContains(code, "Id = 1001");
            AssertContains(code, "Name = @\"EmitterBasic\"");
            AssertContains(code, "Child = new global::ET.Test.CSharpObjectCodeEmitterConcreteChild");
            AssertContains(code, "Label = @\"Child_A\"");

            return ErrorCode.ERR_Success;
        }

        private static void AssertContains(string text, string expected)
        {
            if (!text.Contains(expected, StringComparison.Ordinal))
            {
                throw new Exception($"expected output to contain: {expected}\nactual:\n{text}");
            }
        }
    }
}
