using System;
using System.Collections.Generic;

namespace ET
{
    public class HashSetComponent<T>: HashSet<T>, IPool
    {
        public static HashSetComponent<T> Create()
        {
            return ObjectPool.Fetch(typeof (HashSetComponent<T>)) as HashSetComponent<T>;
        }

        public void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }
            
            this.Clear();
            ObjectPool.Recycle(this);
        }

        public bool IsFromPool { get; set; }
    }
}