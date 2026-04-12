namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class RouterManagerAddressComponent : Entity, IAwake
    {
        public string Address { get; set; }
    }
}
