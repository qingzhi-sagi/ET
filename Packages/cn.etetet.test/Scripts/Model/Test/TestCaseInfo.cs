namespace ET.Test
{
    [EnableClass]
    public sealed class TestCaseInfo
    {
        public string Name { get; }
        public ITestHandler Handler { get; }
        public TestExecutionMode Mode { get; }

        public TestCaseInfo(string name, ITestHandler handler, TestExecutionMode mode)
        {
            this.Name = name;
            this.Handler = handler;
            this.Mode = mode;
        }
    }
}
