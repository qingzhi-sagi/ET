using System;
using System.Collections.Generic;
using System.Reflection;
using Luban;
using SimpleJSON;

namespace ET
{
    internal static class ConfigLoaderHelper
    {
        public static List<Type> GetConfigTypes()
        {
            List<Type> configTypes = new(CodeTypes.Instance.GetTypes(typeof(ConfigProcessAttribute)));
            configTypes.Sort((left, right) => string.CompareOrdinal(left.FullName, right.FullName));
            return configTypes;
        }

        public static void ApplyLoadedConfigs(List<ASingleton> loadedSingletons, Dictionary<Type, IConfig> loadedConfigs)
        {
            foreach (ASingleton singleton in loadedSingletons)
            {
                World.Instance.ReplaceSingleton(singleton);
            }

            foreach (IConfig config in loadedConfigs.Values)
            {
                config.ResolveRef();
            }
        }

        public static bool HasCodeConfig(List<Type> configTypes)
        {
            foreach (Type configType in configTypes)
            {
                ConfigProcessAttribute configProcessAttribute = configType.GetCustomAttributes(typeof(ConfigProcessAttribute), false)[0] as ConfigProcessAttribute;
                if (configProcessAttribute.ConfigType == ConfigType.Code)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsStartConfig(string configName)
        {
            switch (configName)
            {
                case "StartMachineConfigCategory":
                case "StartProcessConfigCategory":
                case "StartSceneConfigCategory":
                case "StartZoneConfigCategory":
                    return true;
                default:
                    return false;
            }
        }

        public static Assembly GetLoadedConfigAssembly()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "ET.Config")
                {
                    return assembly;
                }
            }

            return null;
        }

        public static object DeserializeConfig(Type configType, int configDataType, object configData)
        {
            switch (configDataType)
            {
                case ConfigType.Luban:
                    return Activator.CreateInstance(configType, new ByteBuf((byte[])configData));
                case ConfigType.Json:
                    return Activator.CreateInstance(configType, (object)JSON.Parse((string)configData));
                case ConfigType.Bson:
                    return MongoHelper.FromJson(configType, (string)configData);
                default:
                    throw new Exception($"unsupported config type: {configDataType} {configType.FullName}");
            }
        }

        public static Dictionary<Type, IConfigFactory> CreateConfigFactories(IEnumerable<Type> allTypes)
        {
            List<Type> factoryTypes = new();
            foreach (Type type in allTypes)
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
            Dictionary<Type, IConfigFactory> factories = new();
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

        public static ASingleton LoadOneConfig(Type configType, Dictionary<Type, object> configBytes, Dictionary<Type, IConfigFactory> configFactories, Dictionary<Type, IConfig> loadedConfigs)
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

                category = DeserializeConfig(configType, configProcessAttribute.ConfigType, oneConfigBytes);
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
