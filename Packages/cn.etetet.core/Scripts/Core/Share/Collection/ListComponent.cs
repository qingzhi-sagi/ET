using System;
using System.Collections.Generic;

namespace ET
{
    public class ListComponent<T>: List<T>, IPool
    {
        public static ListComponent<T> Create()
        {
            return ObjectPool.Fetch(typeof (ListComponent<T>)) as ListComponent<T>;
        }

        public void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }
            
            if (this.Capacity > 64) // 超过64，让gc回收
            {
                return;
            }
            this.Clear();
            ObjectPool.Recycle(this);
        }

        public bool IsFromPool { get; set; }
    }
}