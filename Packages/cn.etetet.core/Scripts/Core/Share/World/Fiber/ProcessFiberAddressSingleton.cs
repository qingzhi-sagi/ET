using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ET
{
    [CodeProcess]
    public class ProcessFiberAddressSingleton : Singleton<ProcessFiberAddressSingleton>, ISingletonAwake, IInheritableSingleton
    {
        private readonly ConcurrentDictionary<int, FiberInstanceId> fiberInstanceIds = new();

        public void Awake()
        {
        }

        public void Register(int sceneType, FiberInstanceId fiberInstanceId)
        {
            if (sceneType == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sceneType), sceneType, "process fiber scene type cannot be 0");
            }

            if (fiberInstanceId == default)
            {
                throw new ArgumentException("process fiber instance id cannot be default", nameof(fiberInstanceId));
            }

            if (!this.fiberInstanceIds.TryGetValue(sceneType, out FiberInstanceId existing))
            {
                this.fiberInstanceIds.TryAdd(sceneType, fiberInstanceId);
                return;
            }

            if (existing == fiberInstanceId)
            {
                return;
            }

            throw new Exception(
                $"process fiber address already registered, sceneType: {sceneType}, existing: {existing}, new: {fiberInstanceId}");
        }

        public FiberInstanceId Get(int sceneType)
        {
            if (this.TryGet(sceneType, out FiberInstanceId fiberInstanceId))
            {
                return fiberInstanceId;
            }

            throw new Exception($"process fiber address not found, sceneType: {sceneType}");
        }

        public bool TryGet(int sceneType, out FiberInstanceId fiberInstanceId)
        {
            return this.fiberInstanceIds.TryGetValue(sceneType, out fiberInstanceId);
        }

        public void Unregister(int sceneType, FiberInstanceId fiberInstanceId)
        {
            if (!this.fiberInstanceIds.TryGetValue(sceneType, out FiberInstanceId existing))
            {
                return;
            }

            if (existing != fiberInstanceId)
            {
                return;
            }

            this.fiberInstanceIds.TryRemove(sceneType, out _);
        }
    }
}
