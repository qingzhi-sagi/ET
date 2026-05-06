using System;

namespace ET.Test
{
    [EnableClass]
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TestExecutionAttribute : Attribute
    {
        public TestExecutionMode Mode { get; }

        public TestExecutionAttribute(TestExecutionMode mode)
        {
            this.Mode = mode;
        }
    }
}
