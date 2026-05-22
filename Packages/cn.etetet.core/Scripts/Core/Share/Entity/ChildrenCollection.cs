using System.Diagnostics;

namespace ET
{
    internal sealed class ChildrenCollectionDebugView    
    {
        private readonly ChildrenCollection _collection;

        public ChildrenCollectionDebugView(ChildrenCollection collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Entity[] Items
        {
            get
            {
                Entity[] items = new Entity[_collection.Count];
                _collection.Values.CopyTo(items, 0);
                return items;
            }
        }
    }
    
    [DebuggerTypeProxy(typeof(ChildrenCollectionDebugView))]
    [DebuggerDisplay("Count = {Count}")]
    public class ChildrenCollection : SortedDictionary<long, Entity>, IPool
    {
        public static ChildrenCollection Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ChildrenCollection>(isFromPool);
        }
        
        public bool IsFromPool { get; set; }

        void IPool.Clear()
        {
            base.Clear();
        }
        
        public void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }
}
