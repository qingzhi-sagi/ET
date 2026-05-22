using System;
using System.Collections.Generic;

namespace ET
{
    [DisableNew]
    public class HashSetComponent<T>: HashSet<T>, IPool, IDisposable
    {
        public static HashSetComponent<T> Create()
        {
            return ObjectPool.Fetch(typeof (HashSetComponent<T>)) as HashSetComponent<T>;
        }

        void IPool.Clear()
        {
            base.Clear();
        }

        public void Dispose()
        {
            ObjectPool.Recycle(this);
        }

        public bool IsFromPool { get; set; }
    }
}
