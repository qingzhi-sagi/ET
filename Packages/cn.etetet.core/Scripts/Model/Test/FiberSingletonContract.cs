namespace ET.Test
{
    public class FiberSingletonContract :
            Singleton<FiberSingletonContract>,
            ISingletonAwake<int, string>
    {
        public int Id;

        public string Name;

        public void Awake(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}
