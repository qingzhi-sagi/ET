using System;
using System.Collections.Generic;

namespace ET
{
    [ChildOf(typeof(CoroutineLockQueueType))]
    public partial class CoroutineLockQueue: Entity, IAwake<long>, IDestroy, IPool
    {
        internal long type;

        internal int maxConcurrency { get; set; }

        internal int runningCount;

        internal Queue<ETTask<EntityRef<CoroutineLock>>> queue = new();

        void IPool.Clear()
        {
            this.type = 0;
            this.maxConcurrency = 0;
            this.runningCount = 0;
            this.queue.Clear();
        }

        internal int Count
        {
            get
            {
                return this.queue.Count;
            }
        }
    }
}
