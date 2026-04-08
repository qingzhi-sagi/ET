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
            AssemblyLoadContext loadContext = null;
            try
            {
                ConfigFactoryGroupCollection configFactories = this.LoadConfigFactories(configTypes, out loadContext);
                Dictionary<Type, IConfig> loadedConfigs = new(configTypes.Count);
                List<ASingleton> loadedSingletons = new(configTypes.Count);

                foreach (Type configType in configTypes)
                {
                    ASingleton singleton = ConfigLoaderHelper.LoadOneConfig(configType, configFactories, loadedConfigs);
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

        private ConfigFactoryGroupCollection LoadConfigFactories(List<Type> configTypes, out AssemblyLoadContext loadContext)
        {
            loadContext = null;
            if (!ConfigLoaderHelper.HasCodeConfig(configTypes))
            {
                return new ConfigFactoryGroupCollection();
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
