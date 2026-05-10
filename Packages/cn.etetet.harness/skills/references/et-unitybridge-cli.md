# ET UnityBridge 参考

## CLI 入口

```powershell
dotnet ./Bin/ET.UnityBridge.dll
```

- 默认桥接根目录：优先读取环境变量 `ET_UNITY_BRIDGE_ROOT`。
- 未设置环境变量时，默认 `Temp/UnityBridge`。
- 显式指定根目录：`--root <路径>`。
- 如果 dll 尚不存在，先用 `et-build` 编译确认工具是否生成。

## 心跳检查

```powershell
dotnet ./Bin/ET.UnityBridge.dll heartBeat
```

```powershell
dotnet ./Bin/ET.UnityBridge.dll heartBeat --root "Temp/UnityBridge"
```

用途：

- 判断 UnityBridge 宿主是否在线。
- 获取心跳时间。
- 获取 `IsCompiling` / `IsPlaying` / `IsPlayingOrWillChangePlaymode` / `CodeMode` / `UnityVersion`。

## JSON 请求

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Ping"}'
```

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"HostState"}' --root "Temp/UnityBridge" --waitMs 15000 --timeoutMs 10000 --idempotencyKey "host-state-check"
```

支持的传输参数：

- `--root`
- `--waitMs`
- `--timeoutMs`
- `--idempotencyKey`

## 常用命令

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Ping"}'
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"HostState"}'
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Compile"}'
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Refresh"}'
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"RegenProject"}'
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"EnterPlay"}'
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"ExitPlay"}'
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Reload"}'
```

## 推荐顺序

- 编译前：`HostState -> Compile`。
- 刷新前：`HostState -> Refresh`。
- 重建工程文件：`HostState -> RegenProject`。
- 进播放前：确认 `IsCompiling == false` 且 `IsPlayingOrWillChangePlaymode == false`，再 `EnterPlay`。
- 热重载前：确认 `IsPlaying == true`，再 `Reload`。
- 退播放前：`HostState -> ExitPlay`。

## 实时等待

1. 先执行 `HostState` 或 `heartBeat`。
2. 若 `IsCompiling == true`，持续轮询直到编译结束。
3. 再发 `Compile` / `EnterPlay` / `Reload` / `ExitPlay` 等正式命令。

## 常见错误

- `heartBeat not found`：Unity 未打开、项目未加载完、桥接根目录不一致。
- `unity is compiling`：Unity 正在编译，暂时不能开始新的延迟命令。
- `unity already in playmode or changing playmode`：不能再次执行 `EnterPlay`。
- `unity not in playmode`：执行 `ExitPlay` 或 `Reload` 的前置条件不满足。
- `execute menu item failed`：菜单路径不存在，或 Unity 当前状态不允许执行。

## 输出解读

- `Error == 0` 表示成功。
- 非 0 代表失败，结合 `Message` 判断原因。
- `CompileResponse` 额外包含 `DurationMs`。
- `EnterPlayResponse` / `ExitPlayResponse` 额外包含 `IsPlaying`。
- `HostStateResponse` 额外包含 `AvailableCommands`。
