using System;

namespace ET
{
    /// <summary>
    /// 每个XXConfigCategory单例的接口
    /// </summary>
    public interface IConfig
    {
        //Model中的所有配置初始化完毕后的调用
        //Ref的初始化
        void ResolveRef();
    }

    /// <summary>
    /// 代码配置工厂接口，由Hotfix返回已经填充完数据的配置单例实例。
    /// </summary>
    public interface IConfigFactory
    {
        Type ConfigType { get; }

        ASingleton Create();
    }
}
