using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using MemoryPack;
using TrueSync;

namespace ET
{
    public static class LSWorldSystem
    {
        public static LSWorld LSWorld(this LSEntity entity)
        {
            return entity.IScene as LSWorld;
        }

        public static long GetId(this LSEntity entity)
        {
            return entity.LSWorld().GetId();
        }
        
        public static TSRandom GetRandom(this LSEntity entity)
        {
            return entity.LSWorld().Random;
        }
    }

    [EnableMethod]
    [ChildOf]
    [MemoryPackable]
    public partial class LSWorld: Entity, IAwake, IScene
    {
        [MemoryPackConstructor]
        public LSWorld()
        {
        }
        
        public LSWorld(int sceneType)
        {
            this.Id = this.GetId();

            this.SceneType = sceneType;
        }

        private readonly LSUpdater updater = new();
        
        [BsonIgnore]
        [MemoryPackIgnore]
        public Fiber Fiber { get; set; }
        
        [BsonElement]
        [MemoryPackInclude]
        private long idGenerator;

        public long GetId()
        {
            return ++this.idGenerator;
        }

        public TSRandom Random { get; set; }
        
        [BsonIgnore]
        [MemoryPackIgnore]
        public int SceneType { get; set; }
        
        public int Frame { get; set; }

        public void Update()
        {
            this.updater.Update();
            ++this.Frame;
        }

        public void RegisterSystem(LSEntity entity)
        {
            this.updater.Add(entity);
        }
        
        // lsworld自己时挂在entity上的，但是自己的组件却是LSEntity，所以这里两个get hash code的实现不一样
        public override long GetLongHashCode()
        {
            return this.GetType().TypeHandle.Value.ToInt64();
        }
        
        public override long GetComponentLongHashCode(Type type)
        {
            return LSEntitySystemSingleton.Instance.GetLongHashCode(type);
        }
        
        
        public new K AddComponent<K>(bool isFromPool = false) where K : LSEntity, IAwake, new()
        {
            Entity component = this.CreateComponent(typeof (K), this.GetId(), isFromPool);
            EntitySystemSingleton.Instance.Awake(component);
            return component as K;
        }

        public new K AddComponent<K, P1>(P1 p1, bool isFromPool = false) where K : LSEntity, IAwake<P1>, new()
        {
            Entity component = this.CreateComponent(typeof (K), this.GetId(), isFromPool);
            EntitySystemSingleton.Instance.Awake(component, p1);
            return component as K;
        }

        public new K AddComponent<K, P1, P2>(P1 p1, P2 p2, bool isFromPool = false) where K : LSEntity, IAwake<P1, P2>, new()
        {
            Entity component = this.CreateComponent(typeof (K), this.GetId(), isFromPool);
            EntitySystemSingleton.Instance.Awake(component, p1, p2);
            return component as K;
        }

        public new K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : LSEntity, IAwake<P1, P2, P3>, new()
        {
            Entity component = this.CreateComponent(typeof (K), this.GetId(), isFromPool);
            EntitySystemSingleton.Instance.Awake(component, p1, p2, p3);
            return component as K;
        }

        public new T AddChild<T>(bool isFromPool = false) where T : LSEntity, IAwake
        {
            T component = (T) this.CreateChild(typeof (T), this.GetId(), isFromPool);
            EntitySystemSingleton.Instance.Awake(component);
            return component;
        }

        public new T AddChild<T, A>(A a, bool isFromPool = false) where T : LSEntity, IAwake<A>
        {
            T component = (T) this.CreateChild(typeof (T), this.GetId(), isFromPool);
            EntitySystemSingleton.Instance.Awake(component, a);
            return component;
        }

        public new T AddChild<T, A, B>(A a, B b, bool isFromPool = false) where T : LSEntity, IAwake<A, B>
        {
            T component = (T) this.CreateChild(typeof (T), this.GetId(), isFromPool);
            EntitySystemSingleton.Instance.Awake(component, a, b);
            return component;
        }

        public new T AddChild<T, A, B, C>(A a, B b, C c, bool isFromPool = false) where T : LSEntity, IAwake<A, B, C>
        {
            T component = (T) this.CreateChild(typeof (T), this.GetId(), isFromPool);
            EntitySystemSingleton.Instance.Awake(component, a, b, c);
            return component;
        }

    }
}
