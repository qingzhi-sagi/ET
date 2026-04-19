using System;
using System.Collections.Generic;

namespace ET.Test
{
    public class Config_CSharpObjectCodeEmitter_Advanced_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            VerifyConstructorAndPropertyOutput();
            VerifyIgnoreAndDictionaryOutput();
            VerifyMissingConstructorError();
            VerifyCycleError();

            return ErrorCode.ERR_Success;
        }

        private static void VerifyConstructorAndPropertyOutput()
        {
            CSharpObjectCodeEmitter emitter = new();
            CSharpObjectCodeEmitterConstructorSample sample = new(7)
            {
                Name = "Ctor",
                PublicProp = 9,
            };

            string code = emitter.Emit(sample);

            AssertContains(code, "new global::ET.Test.CSharpObjectCodeEmitterConstructorSample(7)");
            AssertContains(code, "Name = @\"Ctor\"");
            AssertContains(code, "PublicProp = 9");
        }

        private static void VerifyIgnoreAndDictionaryOutput()
        {
            CSharpObjectCodeEmitter emitter = new();
            CSharpObjectCodeEmitterPropertySample sample = new()
            {
                PublicProp = 3,
                NullableText = null,
                Count = 5,
                Map = new Dictionary<int, string>
                {
                    [2] = "B",
                    [1] = "A",
                },
            };

            string code = emitter.Emit(sample);

            AssertContains(code, "PublicProp = 3");
            AssertContains(code, "{ 1, @\"A\" }");
            AssertContains(code, "{ 2, @\"B\" }");
            AssertNotContains(code, "NullableText");
            AssertNotContains(code, "Count = 5");
        }

        private static void VerifyMissingConstructorError()
        {
            CSharpObjectCodeEmitter emitter = new();
            CSharpObjectCodeEmitterMissingCtorSample sample = new();
            Exception exception = AssertThrows(() => emitter.Emit(sample));

            AssertContains(exception.Message, typeof(CSharpObjectCodeEmitterMissingCtorSample).FullName);
            AssertContains(exception.Message, "hidden");
            AssertContains(exception.Message, "[BsonConstructor]");
        }

        private static void VerifyCycleError()
        {
            CSharpObjectCodeEmitter emitter = new();
            CSharpObjectCodeEmitterCycleSample sample = new();
            sample.Next = sample;

            Exception exception = AssertThrows(() => emitter.Emit(sample));
            AssertContains(exception.Message, "循环引用");
        }

        private static Exception AssertThrows(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                return exception;
            }

            throw new Exception("expected exception but action completed successfully");
        }

        private static void AssertContains(string text, string expected)
        {
            if (!text.Contains(expected, StringComparison.Ordinal))
            {
                throw new Exception($"expected text to contain: {expected}\nactual:\n{text}");
            }
        }

        private static void AssertNotContains(string text, string unexpected)
        {
            if (text.Contains(unexpected, StringComparison.Ordinal))
            {
                throw new Exception($"expected text to not contain: {unexpected}\nactual:\n{text}");
            }
        }
    }
}
