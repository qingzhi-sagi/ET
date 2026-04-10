# et-unitybridge - UnityBridge 命令调用专家

这个 skill 专门负责通过 `ET.UnityBridge` 命令行入口与 Unity Editor 文件桥接宿主交互，适合 AI 直接调用现有 UnityBridge 命令。

## 使用场景

- 查询 UnityBridge 宿主是否在线
- 实时轮询 Unity Editor 心跳与状态
- 查询 Unity 当前编译状态、PlayMode 状态、CodeMode、Unity 版本
- 触发 Unity 脚本编译
- 触发 AssetDatabase 刷新
- 触发工程文件重建
- 进入 PlayMode
- 退出 PlayMode
- 在 PlayMode 中执行 Hotfix Reload
- 排查 UnityBridge 返回的 `Error` / `Message`

## 命令执行规范

**重要：本项目所有命令都必须使用 PowerShell。**

- CLI 入口：`dotnet ./Bin/ET.UnityBridge.dll`
- 默认桥接根目录：优先读取环境变量 `ET_UNITY_BRIDGE_ROOT`
- 如果未设置环境变量，则默认使用 `Temp/UnityBridge`
- 如需显式指定根目录，统一使用 `--root <路径>`

## 快速规则

- 调用桥接前，优先先查 `heartBeat` 或 `HostState`
- `Compile`、`Refresh`、`RegenProject`、`EnterPlay`、`ExitPlay` 是延迟完成命令，CLI 会等待结果返回
- `Reload` 不是延迟命令，但要求 Unity 已进入 PlayMode
- 如果桥接宿主不可用，先看 `heartBeat` 是否存在，再看 Unity 是否已打开项目

## 标准调用格式

### 1. 心跳检查（推荐优先）

```powershell
dotnet ./Bin/ET.UnityBridge.dll heartBeat
```

显式指定根目录：

```powershell
dotnet ./Bin/ET.UnityBridge.dll heartBeat --root "Temp/UnityBridge"
```

用途：

- 判断 UnityBridge 宿主是否在线
- 获取实时心跳时间
- 获取 `IsCompiling` / `IsPlaying` / `IsPlayingOrWillChangePlaymode` / `CodeMode` / `UnityVersion`

### 2. 直接发送 JSON 请求

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{\"_t\":\"Ping\"}'
```

可选传输参数：

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{\"_t\":\"HostState\"}' --root "Temp/UnityBridge" --waitMs 15000 --timeoutMs 10000 --idempotencyKey "host-state-check"
```

支持的传输参数：

- `--root`
- `--waitMs`
- `--timeoutMs`
- `--idempotencyKey`

## 常用命令模板

### Ping

最轻量的宿主连通性检查。

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{\"_t\":\"Ping\"}'
```

### HostState

读取宿主当前能力与状态，适合在执行具体操作前先探测。

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{\"_t\":\"HostState\"}'
```

返回重点字段：

- `IsCompiling`
- `IsPlaying`
- `IsPlayingOrWillChangePlaymode`
- `CodeMode`
- `UnityVersion`
- `AvailableCommands`

### Compile

触发 Unity 菜单 `ET/Scripts/Compile`，属于延迟完成命令。

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{\"_t\":\"Compile\"}'
```

适用场景：

- 代码修改后希望让 Unity 侧执行正式编译
- 需要等待编译完成并获得成功/失败结果

### Refresh

触发 `AssetDatabase.Refresh(ForceUpdate)`，属于延迟完成命令。

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{\"_t\":\"Refresh\"}'
```

### RegenProject

触发 Unity 菜单 `ET/Loader/ReGenerateProjectFiles`，属于延迟完成命令。

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{\"_t\":\"RegenProject\"}'
```

### EnterPlay

请求 Unity 进入 PlayMode，属于延迟完成命令。

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{\"_t\":\"EnterPlay\"}'
```

调用前建议先确认：

- `IsCompiling == false`
- `IsPlayingOrWillChangePlaymode == false`

### ExitPlay

请求 Unity 退出 PlayMode，属于延迟完成命令。

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{\"_t\":\"ExitPlay\"}'
```

### Reload

在 PlayMode 中执行 Unity 菜单 `ET/Scripts/Reload`。

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{\"_t\":\"Reload\"}'
```

调用前建议先确认：

- `IsPlaying == true`

## 实时工作流

### 只判断宿主是否在线

1. 先执行 `heartBeat`
2. 如果返回心跳 JSON，说明 UnityBridge 宿主已在线
3. 如果提示未找到心跳文件，优先检查 Unity 是否已打开且项目是否已完成加载

### 实时等待 Unity 状态变化

1. 先执行 `HostState` 或 `heartBeat`
2. 若 `IsCompiling == true`，持续轮询直到编译结束
3. 再发 `Compile` / `EnterPlay` / `Reload` / `ExitPlay` 等正式命令

### 推荐顺序

- 编译前：`HostState -> Compile`
- 进播放前：`HostState -> EnterPlay`
- 热重载前：`HostState -> Reload`
- 退播放前：`HostState -> ExitPlay`

## 常见错误判断

### `heartBeat not found`

说明心跳文件尚未生成，通常表示：

- Unity 没有打开该项目
- Unity 还没完成加载
- 桥接根目录不一致

### `unity is compiling`

说明 Unity 当前正在编译，暂时不能开始新的延迟命令。

### `unity already in playmode or changing playmode`

说明当前已在 PlayMode，或正在切换中，不能再次执行 `EnterPlay`。

### `unity not in playmode`

说明当前不在 PlayMode，通常是执行 `ExitPlay` 或 `Reload` 的前置条件不满足。

### `execute menu item failed`

说明 Unity 菜单项执行失败，优先检查：

- 菜单路径是否存在
- Unity 是否处于允许执行该操作的状态
- 上一次编译或切模式是否尚未结束

## 输出解读

- 返回 `Error == 0` 表示成功
- 返回非 0 代表失败，需要结合 `Message` 判断原因
- `CompileResponse` 额外包含 `DurationMs`
- `EnterPlayResponse` / `ExitPlayResponse` 额外包含 `IsPlaying`
- `HostStateResponse` 额外包含 `AvailableCommands`

## 建议

- 需要低成本探活时优先用 `heartBeat`
- 需要完整上下文时优先用 `HostState`
- 对会改变 Unity 状态的命令，先读状态，再执行命令
- 若用户要求“实时”监控，默认理解为轮询 `heartBeat` 或 `HostState`
