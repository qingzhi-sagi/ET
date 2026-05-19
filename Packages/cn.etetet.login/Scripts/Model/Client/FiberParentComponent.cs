namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class FiberParentComponent: Entity, IAwake
    {
        public long ParentFiberId { get; set; }
    }
}
