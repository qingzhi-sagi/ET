using System;
using System.Collections.Generic;
using System.IO;
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
            Dictionary<Type, object> configBytes = await this.LoadConfigBytesAsync(configTypes);
            Dictionary<Type, IConfigFactory> configFactories = await this.LoadConfigFactoriesAsync(configTypes);
            Dictionary<Type, IConfig> loadedConfigs = new(configTypes.Count);
            List<ASingleton> loadedSingletons = new(configTypes.Count);

            foreach (Type configType in configTypes)
            {
                ASingleton singleton = ConfigLoaderHelper.LoadOneConfig(configType, configBytes, configFactories, loadedConfigs);
                loadedSingletons.Add(singleton);
            }

            ConfigLoaderHelper.ApplyLoadedConfigs(loadedSingletons, loadedConfigs);
        }

        private async ETTask<Dictionary<Type, object>> LoadConfigBytesAsync(List<Type> configTypes)
        {
            Dictionary<Type, object> output = new(configTypes.Count);

#if UNITY_EDITOR
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            string codeMode = globalConfig.CodeMode.ToString();

            foreach (Type configType in configTypes)
            {
                ConfigProcessAttribute configProcessAttribute = configType.GetCustomAttributes(typeof(ConfigProcessAttribute), false)[0] as ConfigProcessAttribute;
                switch (configProcessAttribute.ConfigType)
                {
                    case ConfigType.Luban:
                        output[configType] = File.ReadAllBytes($"Packages/cn.etetet.excel/Bundles/Luban/Config/{codeMode}/Binary/{configType.Name}.bytes");
                        break;
                    case ConfigType.Json:
                        if (ConfigLoaderHelper.IsStartConfig(configType.Name))
                        {
                            output[configType] = File.ReadAllText($"Packages/cn.etetet.startconfig/Bundles/Luban/{Options.Instance.StartConfig}/Server/Json/{configType.Name}.txt");
                        }
                        else
                        {
                            output[configType] = File.ReadAllText($"Packages/cn.etetet.excel/Bundles/Luban/Config/{codeMode}/Json/{configType.Name}.txt");
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
#else
            foreach (Type configType in configTypes)
            {
                ConfigProcessAttribute configProcessAttribute = configType.GetCustomAttributes(typeof(ConfigProcessAttribute), false)[0] as ConfigProcessAttribute;
                if (configProcessAttribute.ConfigType == ConfigType.Code)
                {
                    continue;
                }

                TextAsset asset = await ResourcesComponent.Instance.LoadAssetAsync<TextAsset>(configType.Name);
                switch (configProcessAttribute.ConfigType)
                {
                    case ConfigType.Luban:
                        output[configType] = asset.bytes;
                        break;
                    case ConfigType.Json:
                    case ConfigType.Bson:
                        output[configType] = asset.text;
                        break;
                }
            }
#endif

            return output;
        }

        private async ETTask<Dictionary<Type, IConfigFactory>> LoadConfigFactoriesAsync(List<Type> configTypes)
        {
            if (!ConfigLoaderHelper.HasCodeConfig(configTypes))
            {
                return new Dictionary<Type, IConfigFactory>();
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
