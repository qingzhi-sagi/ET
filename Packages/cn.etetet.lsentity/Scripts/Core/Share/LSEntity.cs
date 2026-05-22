using System;

namespace ET
{
    [EnableMethod]
    public abstract partial class LSEntity: Entity
    {
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

        [EnableAccessEntiyChild]
        public new T AddChild<T>(bool isFromPool = false) where T : LSEntity, IAwake
        {
            T component = (T) this.CreateChild(typeof (T), this.GetId(), isFromPool);
            EntitySystemSingleton.Instance.Awake(component);
            return component;
        }

        [EnableAccessEntiyChild]
        public new T AddChild<T, A>(A a, bool isFromPool = false) where T : LSEntity, IAwake<A>
        {
            T component = (T) this.CreateChild(typeof (T), this.GetId(), isFromPool);
            EntitySystemSingleton.Instance.Awake(component, a);
            return component;
        }

        [EnableAccessEntiyChild]
        public new T AddChild<T, A, B>(A a, B b, bool isFromPool = false) where T : LSEntity, IAwake<A, B>
        {
            T component = (T) this.CreateChild(typeof (T), this.GetId(), isFromPool);
            EntitySystemSingleton.Instance.Awake(component, a, b);
            return component;
        }

        [EnableAccessEntiyChild]
        public new T AddChild<T, A, B, C>(A a, B b, C c, bool isFromPool = false) where T : LSEntity, IAwake<A, B, C>
        {
            T component = (T) this.CreateChild(typeof (T), this.GetId(), isFromPool);
            EntitySystemSingleton.Instance.Awake(component, a, b, c);
            return component;
        }

        // LSEntity自身跟Component都是LSEntity，所以这里两个实现一样
        public override long GetLongHashCode()
        {
            return LSEntitySystemSingleton.Instance.GetLongHashCode(this.GetType());
        }
        
        public override long GetComponentLongHashCode(Type type)
        {
            return LSEntitySystemSingleton.Instance.GetLongHashCode(type);
        }

        protected override void RegisterSystem()
        {
            LSWorld lsWorld = (LSWorld)this.IScene;
            TypeSystems.OneTypeSystems oneTypeSystems = LSEntitySystemSingleton.Instance.GetOneTypeSystems(this.GetType());
            if (oneTypeSystems == null)
            {
                return;
            }

            if (oneTypeSystems.ClassType.Contains(typeof(ILSUpdateSystem)))
            {
                lsWorld.RegisterSystem(this);
            }
        }
    }
}
