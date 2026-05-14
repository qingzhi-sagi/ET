# ET UnityBridge 参考

本文件只记录 CLI 传输、等待和错误解读。AI 操作 Unity 的任务路由见 `et-unitybridge-ai-ops.md`。

## CLI 入口

```powershell
dotnet ./Bin/ET.UnityBridge.dll
```

- 默认桥接根目录：优先读取环境变量 `ET_UNITY_BRIDGE_ROOT`。
- 未设置环境变量时，默认 `Temp/UnityBridge`。
- 显式指定根目录：`--root <路径>`。
- 如果 dll 尚不存在，先用 `et-build` 编译确认工具是否生成。

## 连通性检查

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Ping"}'
```

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Ping"}' --root "Temp/UnityBridge"
```

用途：

- 判断 UnityBridge 宿主是否在线。
- 获取响应时间。
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

省 token 规则：不要完整枚举所有请求。需要命令名用 `HostState.AvailableCommands`，需要字段用 `rg` 查 `Packages/cn.etetet.unitybridge/Proto`。

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

更多命令按需发现：

```powershell
rg -n "^message .*Request|^message (Ping|HostState|Compile|Refresh|RegenProject|EnterPlay|ExitPlay|Reload)\b" ./Packages/cn.etetet.unitybridge/Proto
```

## 推荐顺序

- 编译前：`HostState -> Compile`。
- 刷新前：`HostState -> Refresh`。
- 重建工程文件：`HostState -> RegenProject`。
- 进播放前：确认 `IsCompiling == false` 且 `IsPlayingOrWillChangePlaymode == false`，再 `EnterPlay`。
- 热重载前：确认 `IsPlaying == true`，再 `Reload`。
- 退播放前：`HostState -> ExitPlay`。

## 实时等待

1. 先执行 `Ping` 或 `HostState`。
2. 若 `IsCompiling == true`，持续轮询直到编译结束。
3. 再发 `Compile` / `EnterPlay` / `Reload` / `ExitPlay` 等正式命令。
4. deferred 命令必须读取最终响应；不要只看到请求已接收就结束。

## 常见错误

- `wait unity bridge response timeout`：Unity 未打开、项目未加载完、桥接根目录不一致，或 Editor 未处理请求。
- `unity is compiling`：Unity 正在编译，暂时不能开始新的延迟命令。
- `unity already in playmode or changing playmode`：不能再次执行 `EnterPlay`。
- `unity not in playmode`：执行 `ExitPlay` 或 `Reload` 的前置条件不满足。
- `execute menu item failed`：菜单路径不存在，或 Unity 当前状态不允许执行。

## 输出解读

- `Error == 0` 表示成功。
- 非 0 代表失败，结合 `Message` 判断原因。
- `CompileResponse` 额外包含 `DurationMs`。
- `EnterPlayResponse` / `ExitPlayResponse` 额外包含 `IsPlaying`。
- `PingResponse` 额外包含 `Time`、`IsCompiling`、`IsPlaying`、`IsPlayingOrWillChangePlaymode`、`CodeMode`、`UnityVersion`。
- `HostStateResponse` 额外包含 `AvailableCommands`。
