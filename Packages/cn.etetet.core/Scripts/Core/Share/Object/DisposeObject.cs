using System;
using System.ComponentModel;

namespace ET
{
    public abstract class DisposeObject: Object, IDisposable, ISupportInitialize
    {
        public virtual void Dispose()
        {
            ObjectPool.Recycle(this);
        }
        
        public virtual void BeginInit()
        {
        }
        
        public virtual void EndInit()
        {
        }
    }

    public interface IPool
    {
        bool IsFromPool
        {
            get;
            set;
        }

        void Clear();
    }
}
