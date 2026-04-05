using System.Collections.Generic;

namespace ET.Test
{
    [ComponentOf(typeof(Scene))]
    public class TestFiberDatabaseCleanupComponent : Entity, IAwake
    {
        public HashSet<string> LogicalDbNames = new();
    }
}
