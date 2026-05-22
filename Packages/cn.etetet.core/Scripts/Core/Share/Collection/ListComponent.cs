using System;
using System.Collections.Generic;

namespace ET
{
    [DisableNew]
    public class ListComponent<T>: List<T>, IPool, IDisposable
    {
        public static ListComponent<T> Create()
        {
            return ObjectPool.Fetch(typeof (ListComponent<T>)) as ListComponent<T>;
        }

        void IPool.Clear()
        {
            base.Clear();
        }

        public void Dispose()
        {
            if (this.Capacity > 64) // 超过64，让gc回收
            {
                return;
            }
            ObjectPool.Recycle(this);
        }

        public bool IsFromPool { get; set; }
    }
}
