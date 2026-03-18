using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ET
{
    public struct ConfigDeserialize
    {
        public Type Type;
        public object ConfigBytes;
    }

    public class ConfigLoader : Singleton<ConfigLoader>, ISingletonAwake
    {
        public struct ConfigGetAllConfigBytes
        {
        }

        private readonly ConcurrentDictionary<Type, IConfig> allConfig = new();

        public void Awake()
        {
        }

        public async ETTask LoadAsync()
        {
            Dictionary<Type, object> configBytes = await EventSystem.Instance.Invoke<ConfigGetAllConfigBytes, ETTask<Dictionary<Type, object>>>(new ConfigGetAllConfigBytes());
            Dictionary<Type, IConfigFactory> configFactories = this.LoadConfigFactories();
            List<Type> configTypes = this.GetConfigTypes();
            var loadedConfigs = new Dictionary<Type, IConfig>(configTypes.Count);
            var loadedSingletons = new List<ASingleton>(configTypes.Count);

            foreach (Type configType in configTypes)
            {
                ASingleton singleton = this.LoadOneConfig(configType, configBytes, configFactories, loadedConfigs);
                loadedSingletons.Add(singleton);
            }

            foreach (ASingleton singleton in loadedSingletons)
            {
                World.Instance.ReplaceSingleton(singleton);
            }

            foreach (IConfig config in loadedConfigs.Values)
            {
                config.ResolveRef();
            }

            this.allConfig.Clear();
            foreach ((Type type, IConfig config) in loadedConfigs)
            {
                this.allConfig[type] = config;
            }
        }

        private List<Type> GetConfigTypes()
        {
            var configTypes = new List<Type>(CodeTypes.Instance.GetTypes(typeof(ConfigProcessAttribute)));
            configTypes.Sort((left, right) => string.CompareOrdinal(left.FullName, right.FullName));
            return configTypes;
        }

        private Dictionary<Type, IConfigFactory> LoadConfigFactories()
        {
            var factoryTypes = new List<Type>();
            foreach (Type type in CodeTypes.Instance.GetTypes().Values)
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (!typeof(IConfigFactory).IsAssignableFrom(type))
                {
                    continue;
                }

                factoryTypes.Add(type);
            }

            factoryTypes.Sort((left, right) => string.CompareOrdinal(left.FullName, right.FullName));
            var factories = new Dictionary<Type, IConfigFactory>();
            foreach (Type factoryType in factoryTypes)
            {
                if (Activator.CreateInstance(factoryType) is not IConfigFactory factory)
                {
                    throw new Exception($"create config factory failed: {factoryType.FullName}");
                }

                if (factory.ConfigType == null)
                {
                    throw new Exception($"config factory target is null: {factoryType.FullName}");
                }

                if (!factories.TryAdd(factory.ConfigType, factory))
                {
                    throw new Exception($"duplicate config factory: {factory.ConfigType.FullName}");
                }
            }

            return factories;
        }

        private ASingleton LoadOneConfig(Type configType, Dictionary<Type, object> configBytes, Dictionary<Type, IConfigFactory> configFactories, Dictionary<Type, IConfig> loadedConfigs)
        {
            object category;
            ConfigProcessAttribute configProcessAttribute = configType.GetCustomAttributes(typeof(ConfigProcessAttribute), false)[0] as ConfigProcessAttribute;

            if (configProcessAttribute.ConfigType == ConfigType.Code)
            {
                if (!configFactories.TryGetValue(configType, out IConfigFactory factory))
                {
                    throw new Exception($"config factory not found: {configType.FullName}");
                }

                category = factory.Create();
            }
            else
            {
                if (!configBytes.TryGetValue(configType, out object oneConfigBytes))
                {
                    throw new Exception($"config bytes not found: {configType.FullName}");
                }

                category = EventSystem.Instance.Invoke<ConfigDeserialize, object>(configProcessAttribute.ConfigType, new ConfigDeserialize
                {
                    Type = configType,
                    ConfigBytes = oneConfigBytes,
                });
            }

            if (category == null)
            {
                throw new Exception($"config create failed: {configType.FullName}");
            }

            if (category.GetType() != configType)
            {
                throw new Exception($"config type mismatch: expect={configType.FullName} actual={category.GetType().FullName}");
            }

            if (category is not ASingleton singleton)
            {
                throw new Exception($"config singleton invalid: {configType.FullName}");
            }

            if (category is not IConfig iConfig)
            {
                throw new Exception($"config interface invalid: {configType.FullName}");
            }

            loadedConfigs.Add(configType, iConfig);
            return singleton;
        }
    }
}
