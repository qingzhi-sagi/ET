using System;

namespace ET.Test
{
    public class Excel_CSharpObjectCodeEmitter_CollectionSample_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            CSharpObjectCodeEmitter emitter = new();
            CSharpObjectCodeEmitterCollectionSample sample = new();
            sample.Numbers.Add(2);
            sample.Numbers.Add(5);
            sample.Tags.Add("B");
            sample.Tags.Add("A");
            sample.Named[2] = "Two";
            sample.Named[1] = "One";

            string code = emitter.Emit(sample);

            AssertContains(code, "new global::ET.Test.CSharpObjectCodeEmitterCollectionSample");
            AssertContains(code, "Numbers = new global::System.Collections.Generic.List<int>");
            AssertContains(code, "2,");
            AssertContains(code, "5,");
            AssertContains(code, "Tags = new global::System.Collections.Generic.HashSet<string>");
            AssertContains(code, "@\"A\"");
            AssertContains(code, "@\"B\"");
            AssertContains(code, "Named = new global::System.Collections.Generic.Dictionary<int, string>");
            AssertContains(code, "{ 1, @\"One\" }");
            AssertContains(code, "{ 2, @\"Two\" }");

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
