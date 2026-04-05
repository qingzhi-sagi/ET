namespace ET.Test
{
    [ComponentOf(typeof(Scene))]
    public class TestZoneAllocatorComponent : Entity, IAwake
    {
        public const int FirstTestZone = 100;

        public int Zone = FirstTestZone;
    }
}
