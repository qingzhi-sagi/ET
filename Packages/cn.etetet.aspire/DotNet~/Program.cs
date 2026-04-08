using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Aspire.Hosting;

namespace ET.Server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

            World.Instance.AddSingleton<Options>();

            Options.Instance.SceneName = Environment.GetEnvironmentVariable("SceneName");
            Options.Instance.StartConfig = Environment.GetEnvironmentVariable("StartConfig") ?? "Localhost";
            Options.Instance.SingleThread = int.Parse(Environment.GetEnvironmentVariable("SingleThread")  ?? "0");

            string currentDirectory = Directory.GetCurrentDirectory();
            string workDir = Path.Combine(currentDirectory, "../../..");
            Console.WriteLine($"Current working directory: {workDir}");

            Console.WriteLine($"Loading ET configs from config group: {Options.Instance.StartConfig}");

            World.Instance.AddSingleton(CreateStartConfigCategory<StartProcessConfigCategory>(Options.Instance.StartConfig));
            World.Instance.AddSingleton(CreateStartConfigCategory<StartSceneConfigCategory>(Options.Instance.StartConfig));
            World.Instance.AddSingleton(CreateStartConfigCategory<StartMachineConfigCategory>(Options.Instance.StartConfig));
            World.Instance.AddSingleton(CreateStartConfigCategory<StartZoneConfigCategory>(Options.Instance.StartConfig));

            // 为每个进程创建Aspire服务
            foreach ((int processId, StartProcessConfig startProcessConfig) in StartProcessConfigCategory.Instance.GetAll())
            {
                int replicasNum = startProcessConfig.Num > 1 ? startProcessConfig.Num : 1;

                List<StartSceneConfig> processScenes = StartSceneConfigCategory.Instance.GetByProcess(processId);
                StartMachineConfig startMachineConfig = StartMachineConfigCategory.Instance.Get(startProcessConfig.MachineId);

                string innerIP = startMachineConfig.InnerIP;
                string innerPortStr = startProcessConfig.Port.ToString();

                string outerIP = startMachineConfig.OuterIP;
                
                string outerPortStr = "0";
                // 进程只有一个Scene，需要设置Scene OuterPort
                if (processScenes.Count == 1)
                {
                    StartSceneConfig startSceneConfig = processScenes[0];
                    outerPortStr = startSceneConfig.Port.ToString();
                }

                // 为每个副本创建独立的服务
                for (int replicaIndex = 1; replicaIndex <= replicasNum; replicaIndex++)
                {
                    string serviceName = replicasNum > 1 ? $"{startProcessConfig.Name}-{processId}-{replicaIndex}" : $"{startProcessConfig.Name}-{processId}";
                    // workingDirectory使用环境变量指定的路径
                    builder.AddExecutable(serviceName, "dotnet", workDir, "./Bin/ET.App.dll")
                            .WithArgs($"--Process={processId}") // 固定逻辑进程 ID
                            .WithArgs($"--ReplicaIndex={replicaIndex}") // 副本索引
                            .WithArgs($"--SceneName={Options.Instance.SceneName}")
                            .WithArgs($"--StartConfig={Options.Instance.StartConfig}")
                            .WithArgs($"--SingleThread={Options.Instance.SingleThread}")
                            .WithEnvironment("InnerIP", innerIP)
                            .WithEnvironment("InnerPort", innerPortStr)
                            .WithEnvironment("OuterIP", outerIP)
                            .WithEnvironment("OuterPort", outerPortStr)
                            .WithOtlpExporter()
                            .WithEnvironment("ASPIRE_MANAGED", "true");
                }

                // 输出该进程的所有场景信息
                foreach (StartSceneConfig scene in processScenes)
                {
                    Console.WriteLine($"  - Scene: {scene.Name} ({scene.SceneType}) Port: {scene.Port}");
                }
            }

            builder.Build().Run();
        }

        private static TCategory CreateStartConfigCategory<TCategory>(string configGroup) where TCategory : ASingleton
        {
            Assembly configAssembly = GetConfigAssembly();
            foreach (Type type in configAssembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface || !typeof(IConfigFactory).IsAssignableFrom(type))
                {
                    continue;
                }

                ConfigGroupAttribute configGroupAttribute = type.GetCustomAttribute<ConfigGroupAttribute>();
                if (!string.Equals(configGroupAttribute?.Name, configGroup, StringComparison.Ordinal))
                {
                    continue;
                }

                IConfigFactory factory = Activator.CreateInstance(type) as IConfigFactory;
                if (factory?.ConfigType != typeof(TCategory))
                {
                    continue;
                }

                return (TCategory)factory.Create();
            }

            throw new Exception($"start config factory not found: category={typeof(TCategory).FullName}, group={configGroup}");
        }

        private static Assembly GetConfigAssembly()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "ET.Config")
                {
                    return assembly;
                }
            }

            throw new Exception("ET.Config assembly is not loaded");
        }
    }
}
