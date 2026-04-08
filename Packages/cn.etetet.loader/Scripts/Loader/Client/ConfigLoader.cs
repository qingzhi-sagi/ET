using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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
            ConfigFactoryGroupCollection configFactories = await this.LoadConfigFactoriesAsync(configTypes);
            Dictionary<Type, IConfig> loadedConfigs = new(configTypes.Count);
            List<ASingleton> loadedSingletons = new(configTypes.Count);

            foreach (Type configType in configTypes)
            {
                ASingleton singleton = ConfigLoaderHelper.LoadOneConfig(configType, configFactories, loadedConfigs);
                loadedSingletons.Add(singleton);
            }

            ConfigLoaderHelper.ApplyLoadedConfigs(loadedSingletons, loadedConfigs);
        }

        private async ETTask<ConfigFactoryGroupCollection> LoadConfigFactoriesAsync(List<Type> configTypes)
        {
            if (!ConfigLoaderHelper.HasCodeConfig(configTypes))
            {
                return new ConfigFactoryGroupCollection();
            }

            Assembly configAssembly = ConfigLoaderHelper.GetLoadedConfigAssembly();
            if (configAssembly == null)
            {
                TextAsset dll = await ResourcesComponent.Instance.LoadAssetAsync<TextAsset>("ET.Config.dll");
                TextAsset pdb = await ResourcesComponent.Instance.LoadAssetAsync<TextAsset>("ET.Config.pdb");
                configAssembly = Assembly.Load(dll.bytes, pdb.bytes);
            }

            return ConfigLoaderHelper.CreateConfigFactories(configAssembly.GetTypes());
        }
    }
}
