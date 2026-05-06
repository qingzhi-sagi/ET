using System;

namespace ET.Test
{
    [EnableClass]
    public sealed class TestCaseResult
    {
        public string Name { get; }
        public int Error { get; }
        public long ElapsedMilliseconds { get; }
        public Exception Exception { get; }
        public bool IsSuccess => this.Error == 0 && this.Exception == null;

        public TestCaseResult(string name, int error, long elapsedMilliseconds, Exception exception)
        {
            this.Name = name;
            this.Error = error;
            this.ElapsedMilliseconds = elapsedMilliseconds;
            this.Exception = exception;
        }
    }
}
