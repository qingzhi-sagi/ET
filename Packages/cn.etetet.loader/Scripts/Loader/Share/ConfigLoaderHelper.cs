using System;
using System.Collections.Generic;
using System.Reflection;

namespace ET
{
    public sealed class ConfigFactoryGroupCollection
    {
        private readonly Dictionary<Type, Dictionary<string, IConfigFactory>> factories = new();

        public void Add(string configGroup, IConfigFactory factory)
        {
            if (factory.ConfigType == null)
            {
                throw new Exception($"config factory target is null: {factory.GetType().FullName}");
            }

            if (string.IsNullOrWhiteSpace(configGroup))
            {
                throw new Exception($"config group is empty: {factory.GetType().FullName}");
            }

            if (!this.factories.TryGetValue(factory.ConfigType, out Dictionary<string, IConfigFactory> groupFactories))
            {
                groupFactories = new Dictionary<string, IConfigFactory>(StringComparer.Ordinal);
                this.factories.Add(factory.ConfigType, groupFactories);
            }

            if (!groupFactories.TryAdd(configGroup, factory))
            {
                throw new Exception($"duplicate config factory: {factory.ConfigType.FullName} group={configGroup}");
            }
        }

        public bool TryGetFactories(Type configType, out Dictionary<string, IConfigFactory> groupFactories)
        {
            return this.factories.TryGetValue(configType, out groupFactories);
        }
    }

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

        public static ConfigFactoryGroupCollection CreateConfigFactories(IEnumerable<Type> allTypes)
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
            ConfigFactoryGroupCollection factories = new();
            foreach (Type factoryType in factoryTypes)
            {
                if (Activator.CreateInstance(factoryType) is not IConfigFactory factory)
                {
                    throw new Exception($"create config factory failed: {factoryType.FullName}");
                }

                factories.Add(GetConfigGroup(factoryType), factory);
            }

            return factories;
        }

        public static ASingleton LoadOneConfig(Type configType, ConfigFactoryGroupCollection configFactories, Dictionary<Type, IConfig> loadedConfigs)
        {
            ConfigProcessAttribute configProcessAttribute = configType.GetCustomAttributes(typeof(ConfigProcessAttribute), false)[0] as ConfigProcessAttribute;
            if (configProcessAttribute.ConfigType != ConfigType.Code)
            {
                throw new Exception($"unsupported config type: {configProcessAttribute.ConfigType} {configType.FullName}");
            }

            IConfigFactory factory = GetConfigFactory(configType, configFactories);
            object category = factory.Create();
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

        private static string GetConfigGroup(MemberInfo factoryType)
        {
            ConfigGroupAttribute configGroupAttribute = factoryType.GetCustomAttribute<ConfigGroupAttribute>();
            return string.IsNullOrWhiteSpace(configGroupAttribute?.Name) ? ConfigGroupNames.Config : configGroupAttribute.Name;
        }

        private static IConfigFactory GetConfigFactory(Type configType, ConfigFactoryGroupCollection configFactories)
        {
            if (!configFactories.TryGetFactories(configType, out Dictionary<string, IConfigFactory> groupFactories))
            {
                throw new Exception($"config factory not found: {configType.FullName}");
            }

            List<string> activeGroups = GetActiveConfigGroups();
            IConfigFactory match = null;
            string matchedGroup = null;
            foreach (string activeGroup in activeGroups)
            {
                if (!groupFactories.TryGetValue(activeGroup, out IConfigFactory factory))
                {
                    continue;
                }

                if (match != null)
                {
                    throw new Exception(
                        $"config factory group ambiguous: {configType.FullName}, matched groups: {matchedGroup}, {activeGroup}");
                }

                match = factory;
                matchedGroup = activeGroup;
            }

            if (match != null)
            {
                return match;
            }

            throw new Exception(
                $"config factory group not found: {configType.FullName}, active groups: {string.Join(", ", activeGroups)}, available groups: {string.Join(", ", groupFactories.Keys)}");
        }

        private static List<string> GetActiveConfigGroups()
        {
            List<string> groups = new() { ConfigGroupNames.Config };
            string startConfig = Options.Instance?.StartConfig;
            if (!string.IsNullOrWhiteSpace(startConfig) && !string.Equals(startConfig, ConfigGroupNames.Config, StringComparison.Ordinal))
            {
                groups.Add(startConfig);
            }

            return groups;
        }
    }
}
