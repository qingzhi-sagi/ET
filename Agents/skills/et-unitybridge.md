# et-unitybridge - UnityBridge 命令入口

## 何时使用

- 查询 UnityBridge 宿主是否在线
- 轮询 Unity Editor 心跳与状态
- 查询 Unity 编译状态、PlayMode 状态、CodeMode、Unity 版本
- 执行 UnityBridge 命令：`Ping` / `HostState` / `Compile` / `Refresh` / `RegenProject` / `EnterPlay` / `ExitPlay` / `Reload`
- 排查 UnityBridge 返回的 `Error` / `Message`

## 不要加载

- 只是普通 C# 编译（用 `et-build`）
- 只是读写 Excel 或导出 Luban
- 只是纯代码结构分析，不需要 Unity 编辑器参与

## 默认动作

1. CLI 入口为 `dotnet ./Bin/ET.UnityBridge.dll`。
2. 调用桥接前，优先先查 `heartBeat` 或 `HostState`。
3. `Compile`、`Refresh`、`RegenProject`、`EnterPlay`、`ExitPlay` 是延迟完成命令，需要等待结果。
4. `Reload` 要求 Unity 已进入 PlayMode。
5. 如果 `Bin/ET.UnityBridge.dll` 尚不存在，先用 `et-build` 编译确认工具是否生成。
6. 如果桥接宿主不可用，先检查 Unity 是否已打开项目、心跳文件是否存在、桥接根目录是否一致。

## 优先入口

- 心跳：`dotnet ./Bin/ET.UnityBridge.dll heartBeat`
- 状态：`dotnet ./Bin/ET.UnityBridge.dll '{"_t":"HostState"}'`
- 编译：`dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Compile"}'`
- 刷新：`dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Refresh"}'`
- 重建工程文件：`dotnet ./Bin/ET.UnityBridge.dll '{"_t":"RegenProject"}'`
- 进入 PlayMode：`dotnet ./Bin/ET.UnityBridge.dll '{"_t":"EnterPlay"}'`
- 退出 PlayMode：`dotnet ./Bin/ET.UnityBridge.dll '{"_t":"ExitPlay"}'`
- 热重载：`dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Reload"}'`

## 按需补读

- `Agents/skills/references/et-unitybridge-cli.md`：传输参数、实时轮询、返回值解读、常见错误
