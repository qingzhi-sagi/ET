using CommandLine;

namespace ET.Test
{
    public class TestArgs: Object
    {
        [Option('n', "Name", Required = false, Default = ".*")]
        public string Name { get; set; }

        [Option('p', "Parallel", Required = false, Default = false)]
        public bool Parallel { get; set; }

        [Option("MaxConcurrency", Required = false, Default = 0)]
        public int MaxConcurrency { get; set; }
    }
}
