using System.Collections.Generic;

namespace ET.Test
{
    [EnableClass]
    public partial class GeneratedIPoolClearWithBusinessClear: IPool
    {
        public int Value;

        public string Text;

        public bool BusinessClearCalled;

        public bool IsFromPool { get; set; }

        public void Clear()
        {
            this.BusinessClearCalled = true;
        }

        public void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [EnableClass]
    public partial class GeneratedIPoolListClearWithBusinessClear: List<int>, IPool
    {
        public int Value;

        public bool BusinessClearCalled;

        public bool IsFromPool { get; set; }

        public new void Clear()
        {
            this.BusinessClearCalled = true;
        }

        public void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [EnableClass]
    public partial class GeneratedIPoolCollectionMemberClear: IPool
    {
        public int Value;

        public List<int> Items { get; set; } = new();

        public Dictionary<int, string> Names { get; set; } = new();

        public bool IsFromPool { get; set; }

        public void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [ComponentOf(typeof(Scene))]
    public partial class EntityPoolClearTestComponent: Entity, IAwake, IPool
    {
        public int Value;

        public string Text;
    }

    [ComponentOf(typeof(Scene))]
    public partial class EntityPoolClearWithIdTestComponent: Entity, IAwake, IPool
    {
        public int Value;

        public string Text;
    }
    
    [ChildOf(typeof(Scene))]
    public class EntityPoolClearRecorder: Entity, IAwake
    {
        public int DestroySawClearValue;
    }
    
    [ChildOf(typeof(Scene))]
    public partial class EntityPoolClearTestEntity: Entity, IAwake<EntityPoolClearRecorder>, IDestroy, IPool
    {
        public int AwakeValue;
        
        public int DestroyValue;
        
        public int ClearValue;
        
        public string Text;
        
        public EntityRef<EntityPoolClearRecorder> Recorder;
    }
}
