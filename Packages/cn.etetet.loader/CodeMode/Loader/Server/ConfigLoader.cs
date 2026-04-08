using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace ET
{
    [AllowInstance]
    public class ConfigLoader : Singleton<ConfigLoader>, ISingletonAwake
    {
        public void Awake()
        {
        }

        public async ETTask LoadAsync()
        {
            List<Type> configTypes = ConfigLoaderHelper.GetConfigTypes();
            Dictionary<Type, object> configBytes = await this.LoadConfigBytesAsync(configTypes);
            AssemblyLoadContext loadContext = null;
            try
            {
                Dictionary<Type, IConfigFactory> configFactories = this.LoadConfigFactories(configTypes, out loadContext);
                Dictionary<Type, IConfig> loadedConfigs = new(configTypes.Count);
                List<ASingleton> loadedSingletons = new(configTypes.Count);

                foreach (Type configType in configTypes)
                {
                    ASingleton singleton = ConfigLoaderHelper.LoadOneConfig(configType, configBytes, configFactories, loadedConfigs);
                    loadedSingletons.Add(singleton);
                }

                ConfigLoaderHelper.ApplyLoadedConfigs(loadedSingletons, loadedConfigs);
            }
            finally
            {
                if (loadContext != null)
                {
                    loadContext.Unload();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
            }
        }

        private async ETTask<Dictionary<Type, object>> LoadConfigBytesAsync(List<Type> configTypes)
        {
            Dictionary<Type, object> output = new(configTypes.Count);

            foreach (Type configType in configTypes)
            {
                ConfigProcessAttribute configProcessAttribute = configType.GetCustomAttributes(typeof(ConfigProcessAttribute), false)[0] as ConfigProcessAttribute;
                switch (configProcessAttribute.ConfigType)
                {
                    case ConfigType.Luban:
                        output[configType] = File.ReadAllBytes($"Packages/cn.etetet.excel/Bundles/Luban/Config/Server/Binary/{configType.Name}.bytes");
                        break;
                    case ConfigType.Json:
                        if (ConfigLoaderHelper.IsStartConfig(configType.Name))
                        {
                            output[configType] = File.ReadAllText($"Packages/cn.etetet.startconfig/Bundles/Luban/{Options.Instance.StartConfig}/Server/Json/{configType.Name}.txt");
                        }
                        else
                        {
                            output[configType] = File.ReadAllText($"Packages/cn.etetet.excel/Bundles/Luban/Config/Server/Json/{configType.Name}.txt");
                        }
                        break;
                    case ConfigType.Bson:
                        output[configType] = File.ReadAllText($"Packages/cn.etetet.map/Bundles/Json/{configType.Name}.txt");
                        break;
                    case ConfigType.Code:
                        break;
                }
            }

            await ETTask.CompletedTask;
            return output;
        }

        private Dictionary<Type, IConfigFactory> LoadConfigFactories(List<Type> configTypes, out AssemblyLoadContext loadContext)
        {
            loadContext = null;
            if (!ConfigLoaderHelper.HasCodeConfig(configTypes))
            {
                return new Dictionary<Type, IConfigFactory>();
            }

            byte[] dllBytes = File.ReadAllBytes("./Bin/ET.Config.dll");
            byte[] pdbBytes = File.ReadAllBytes("./Bin/ET.Config.pdb");
            loadContext = new AssemblyLoadContext("ET.Config", true);
            using MemoryStream dllStream = new(dllBytes);
            using MemoryStream pdbStream = new(pdbBytes);
            Assembly configAssembly = loadContext.LoadFromStream(dllStream, pdbStream);
            return ConfigLoaderHelper.CreateConfigFactories(configAssembly.GetTypes());
        }
    }
}
