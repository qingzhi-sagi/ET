using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ET
{
    public struct FiberDestroyEvent
    {
    }
    
    public class Fiber: IScheduler
    {
        // 该字段只能框架使用，绝对不能改成public，改了后果自负
        [StaticField]
        [ThreadStatic]
        private static Fiber instance;

        // 绝对禁止逻辑使用！！！
        [StaticField]
        internal static Fiber Instance
        {
            get
            {
                return instance;
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                instance = value;
                instance.Log.SceneName = instance.Name;
                SynchronizationContext.SetSynchronizationContext(instance.ThreadSynchronizationContext);
            }
        }
        
        public bool IsDisposed { get; private set; }
        
        public long Id { get; }

        public int Zone => FiberIdHelper.DecodeZone(this.Id);

        public int LocalSlot => FiberIdHelper.DecodeLocalSlot(this.Id);

        private EntityRef<Scene> root;
        
        public SchedulerType SchedulerType { get; }
        
        public long ParentFiberId { get; }

        private FiberMonitorInfo fiberMonitorInfo;

        private long instanceIdGenerator = 1;
        
        public long NewInstanceId()
        {
            return instanceIdGenerator++;
        }

        public Scene Root
        {
            get
            {
                return this.root;
            }
            private set
            {
                this.root = value;
            }
        }

        public string Name { get; }

        public EntitySystem EntitySystem { get; }
        public Mailboxes Mailboxes { get; private set; }
        public ThreadSynchronizationContext ThreadSynchronizationContext { get; }
        public ILog Log { get; }

        private readonly Queue<ETTask> frameFinishTasks = new();
        
        private readonly Queue<Fiber> schedulerQueue = new();

        private readonly Dictionary<long, Fiber> children = new();
        
        private Dictionary<Type, object> singletons;
        
        internal Fiber(long id, long rootId, int sceneType, string name, SchedulerType schedulerType, Fiber parent)
        {
            this.Id = id;
            this.SchedulerType = schedulerType;
            this.EntitySystem = new EntitySystem();
            this.Mailboxes = new Mailboxes();
            this.ParentFiberId = parent?.Id ?? 0;
            this.Name = name;

            if (schedulerType == SchedulerType.Parent)
            {
                this.Log = parent.Log;
                // 注意调度是Parent的纤程，同步上下文必须是父亲的同步上下文
                // 因为父亲有可能会调用其方法，如果同步上下文不一样则父亲调用的上下文会发生改变
                // 比如Test_CreateRobot_Test中，父亲会调用创建出来的子Fiber的发送消息方法，那么子fiber等call回来之后，当前上下文变成了子fiber的了
                // 这样导致父亲在调用前的上下文跟调用后的上下文发生了改变
                this.ThreadSynchronizationContext = parent.ThreadSynchronizationContext;
            }
            else
            {
                this.ThreadSynchronizationContext = new ThreadSynchronizationContext();
                
                LogInvoker logInvoker = new() { SceneName = name };
                this.Log = EventSystem.Instance.Invoke<LogInvoker, ILog>(logInvoker);
            }
            this.Root = new Scene(this, rootId, sceneType, name);
        }
        
        internal void Update()
        {            
            try
            {                
                Instance = this;

                this.fiberMonitorInfo = new FiberMonitorInfo(this.Name);
                
                int t1 = Environment.TickCount;
                
                this.EntitySystem.Publish(new UpdateEvent());
                
                int t2 = Environment.TickCount;
                this.fiberMonitorInfo.UpdateTimeUsed = t2 - t1;

                int count = this.schedulerQueue.Count;
                while (count-- > 0)
                {
                    Fiber fiber = this.schedulerQueue.Dequeue();
                    
                    if (fiber.IsDisposed)
                    {
                        this.children.Remove(fiber.Id);
                        continue;
                    }
                    
                    this.schedulerQueue.Enqueue(fiber);
                    
                    fiber.Update();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        
        internal void LateUpdate()
        {
            try
            {
                Instance = this;

                int t1 = Environment.TickCount;
                
                this.EntitySystem.Publish(new LateUpdateEvent());
                FrameFinishUpdate();
                this.ThreadSynchronizationContext.Update();
                
                int t2 = Environment.TickCount;
                this.fiberMonitorInfo.LateUpdateTimeUsed = t2 - t1;

                FiberManager.Instance?.AddMonitor(this.Id, ref this.fiberMonitorInfo);

                int count = this.schedulerQueue.Count;
                while (count-- > 0)
                {
                    Fiber fiber = this.schedulerQueue.Dequeue();
                    
                    if (fiber.IsDisposed)
                    {
                        this.children.Remove(fiber.Id);
                        continue;
                    }
                    
                    this.schedulerQueue.Enqueue(fiber);
                    
                    fiber.LateUpdate();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public async ETTask WaitFrameFinish()
        {
            ETTask task = ETTask.Create(true);
            this.frameFinishTasks.Enqueue(task);
            await task;
        }

        private void FrameFinishUpdate()
        {
            while (this.frameFinishTasks.Count > 0)
            {
                ETTask task = this.frameFinishTasks.Dequeue();
                task.SetResult();
            }
        }

        public async ETTask<Fiber> CreateFiber(long rootId, int sceneType, string name)
        {
            Fiber fiber = await FiberManager.Instance.CreateFiber(SchedulerType.Parent, this.Zone, rootId, sceneType, name, this);
            this.children.Add(fiber.Id, fiber);
            return fiber;
        }

        public async ETTask<Fiber> CreateFiber(int zone, long rootId, int sceneType, string name)
        {
            Fiber fiber = await FiberManager.Instance.CreateFiber(SchedulerType.Parent, zone, rootId, sceneType, name, this);
            this.children.Add(fiber.Id, fiber);
            return fiber;
        }

        public async ETTask<long> CreateFiber(SchedulerType schedulerType, long rootId, int sceneType, string name)
        {
            schedulerType = this.NormalizeChildSchedulerType(schedulerType);
            
            Fiber fiber = await FiberManager.Instance.CreateFiber(schedulerType, this.Zone, rootId, sceneType, name, this);
            this.children.Add(fiber.Id, fiber);
            return fiber.Id;
        }
        
        public async ETTask<long> CreateFiber(SchedulerType schedulerType, int zone, long rootId, int sceneType, string name)
        {
            schedulerType = this.NormalizeChildSchedulerType(schedulerType);

            Fiber fiber = await FiberManager.Instance.CreateFiber(schedulerType, zone, rootId, sceneType, name, this);
            this.children.Add(fiber.Id, fiber);
            return fiber.Id;
        }
        
        public async ETTask RemoveFiber(long fiberId)
        {
            if (!this.children.Remove(fiberId, out Fiber fiber))
            {
                return;
            }
            
            // 整个FiberManager释放了
            IScheduler scheduler = fiber;
            if (FiberManager.Instance == null)
            {
                scheduler.Dispose();
                return;
            }
            
            if (fiber.SchedulerType == SchedulerType.Parent)
            {
                foreach (long child in fiber.children.Keys.ToArray())
                {
                    await fiber.RemoveFiber(child);
                }
                await EventSystem.Instance.PublishAsync(fiber.Root, new FiberDestroyEvent());
                scheduler.Dispose();
                return;
            }
            
            TaskCompletionSource<bool> tcs = new();
            // 要扔到fiber线程执行，否则会出现线程竞争
            fiber.ThreadSynchronizationContext.Post(() =>
            {
                FiberDestroy().Coroutine();
                return;

                async ETTask FiberDestroy()
                {
                    foreach (long child in fiber.children.Keys.ToArray())
                    {
                        await fiber.RemoveFiber(child);
                    }
                    Scene fiberRoot = (scheduler as Fiber).Root;
                    await EventSystem.Instance.PublishAsync(fiberRoot, new FiberDestroyEvent());
                    scheduler.Dispose();
                    tcs.SetResult(true);
                }
            });
            await tcs.Task;
        }

        public async ETTask RemoveFibers()
        {
            foreach (long i in this.children.Keys.ToArray())
            {
                await this.RemoveFiber(i);
            }
        }
        
        public Fiber GetFiber(long id)
        {
            if (!this.children.TryGetValue(id, out Fiber fiber))
            {
                return null;
            }

            if (fiber.SchedulerType != SchedulerType.Parent)
            {
                return null;
            }
            return fiber;
        }
        
        public Fiber GetFiber(string name)
        {
            foreach (Fiber fiber in this.children.Values)
            {
                if (fiber.SchedulerType != SchedulerType.Parent)
                {
                    continue;
                }
                if (fiber.Name != name)
                {
                    continue;
                }

                return fiber;
            }
            return null;
        }
        
        public void AddSingleton<T>() where T: ASingleton, ISingletonAwake, new()
        {
            T singleton = new();
            singleton.Awake();
            AddSingleton(singleton);
        }
        
        public void AddSingleton<T, A>(A a) where T: ASingleton, ISingletonAwake<A>, new()
        {
            T singleton = new();
            singleton.Awake(a);
            AddSingleton(singleton);
        }
        
        public void AddSingleton<T, A, B>(A a, B b) where T: ASingleton, ISingletonAwake<A, B>, new()
        {
            T singleton = new();
            singleton.Awake(a, b);
            AddSingleton(singleton);
        }
        
        public void AddSingleton<T, A, B, C>(A a, B b, C c) where T: ASingleton, ISingletonAwake<A, B>, new()
        {
            T singleton = new();
            singleton.Awake(a, b);
            AddSingleton(singleton);
        }

        public void AddSingleton<T>(T singleton) where T : ASingleton
        {
            this.singletons ??= new Dictionary<Type, object>();
            this.singletons[typeof(T)] = singleton;
        }

        public T GetSingleton<T>() where T : Singleton<T>
        {
            if (this.singletons == null)
            {
                return Singleton<T>.Instance;
            }
            
            if (!this.singletons.TryGetValue(typeof(T), out object singleton))
            {
                return Singleton<T>.Instance;
            }
            
            return (T)singleton;
        }

        public bool RemoveSingleton<T>() where T : class
        {
            if (this.singletons == null)
            {
                return false;
            }
            return this.singletons.Remove(typeof(T));
        }

        internal void InheritSingletonsFrom(Fiber parent)
        {
            if (parent?.singletons == null)
            {
                return;
            }

            foreach ((Type type, object singleton) in parent.singletons)
            {
                if (singleton is not IInheritableSingleton)
                {
                    continue;
                }

                this.singletons ??= new Dictionary<Type, object>();
                this.singletons.TryAdd(type, singleton);
            }
        }

        private SchedulerType NormalizeChildSchedulerType(SchedulerType schedulerType)
        {
            if (Options.Instance.SingleThread == 1)
            {
                return SchedulerType.Parent;
            }

            return schedulerType;
        }

        /// <summary>
        /// 禁止逻辑调用此方法，应该由Fiber.Remove去移除一个fiber
        /// </summary>
        void IScheduler.Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            this.IsDisposed = true;
            try
            {
                this.Root.Dispose();
            }
            catch (Exception e)
            {
                this.Log.Error(e);
            }
            
            FiberManager.Instance?.RemoveMonitor(this.Id);
            this.singletons?.Clear();
            
            foreach (long child in this.children.Keys.ToArray())
            {
                this.RemoveFiber(child).Coroutine();
            }
        }

        public void AddToScheduler(Fiber fiber)
        {
            this.schedulerQueue.Enqueue(fiber);
        }
    }
}
