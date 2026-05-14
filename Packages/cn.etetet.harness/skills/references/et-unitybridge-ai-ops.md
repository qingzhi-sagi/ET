# ET UnityBridge AI 操作参考

本文件用于 AI 通过 UnityBridge 操作 Unity Editor。只在需要实际操作 Unity 时读取；不要把它当完整命令手册。

## 核心原则

- 所有命令通过 `pwsh` 执行。
- 先读状态，再执行动作。
- 先用最小读命令确认目标，再执行写命令。
- 只读取相关 proto / handler 小片段；不要全量加载命令。
- deferred 命令要等最终响应，不要看到 pending/deferred 就结束。

## 入口

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Ping"}'
```

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"HostState"}'
```

`Ping` 用来判断 UnityBridge 是否在线并读取编译/PlayMode/CodeMode/Unity 版本。`HostState` 用来额外拿 `AvailableCommands`。

## 命令发现

优先用 `HostState` 看当前可用命令。需要字段时再查 proto：

```powershell
rg -n "^message .*Request|^message (Ping|HostState|Compile|Refresh|RegenProject|EnterPlay|ExitPlay|Reload)\b" ./Packages/cn.etetet.unitybridge/Proto
```

需要行为细节时查 handler：

```powershell
rg -n "class UnityBridge.*Handler|AUnityBridgeDeferredHandler" ./Packages/cn.etetet.unitybridge/Scripts/Editor/Share
```

只打开命中的单个 proto 或 handler，不要整包读取。

## 任务路由

| 目标 | 优先命令族 | 常见前置 |
|---|---|---|
| 状态/连通性 | `Ping`, `HostState`, `EditorGetStateRequest` | 无 |
| 编译/刷新 | `Compile`, `Refresh`, `RegenProject`, `AssetRefreshRequest`, `AssetImportRequest` | `IsCompiling == false` |
| PlayMode/热重载 | `EnterPlay`, `ExitPlay`, `Reload`, `EditorPauseRequest` | 检查 `IsPlaying` / `IsPlayingOrWillChangePlaymode` |
| 资源 | `AssetSearchRequest`, `AssetFindRequest`, `AssetLoadRequest`, `AssetReadTextRequest`, `AssetGetPathRequest` | 先限定 filter/path/count |
| 场景 | `SceneGetHierarchyRequest`, `SceneGetActiveRequest`, `SceneLoadRequest`, `SceneSaveRequest`, `SceneNewRequest` | 写操作前确认当前场景 |
| 选择集 | `SelectionGetRequest`, `SelectionSetRequest`, `SelectionAddRequest`, `SelectionRemoveRequest`, `SelectionClearRequest` | 先读当前 selection |
| 对象/Transform | `GameObject*Request`, `Transform*Request` | 先 `Find/GetInfo/Get` |
| Inspector | `InspectorGet*Request`, `InspectorSet*Request`, `InspectorAddComponentRequest`, `InspectorRemoveComponentRequest` | 先读组件和属性名 |
| Prefab | `PrefabInstantiateRequest`, `PrefabSaveRequest`, `PrefabApplyRequest`, `PrefabGet*Request`, `PrefabUnpackRequest` | 先确认 asset path / instance |
| 截图/GameView | `ScreenshotCaptureRequest`, `GameView*Request` | 先读分辨率 |
| 测试 | `UnityTestRunRequest` | 用精确正则 |
| 批量 | `BatchExecuteRequest` | 先单步验证 |

## 操作模式

### 读状态

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"HostState"}'
```

只总结关键字段：`Error`、`Message`、`IsCompiling`、`IsPlaying`、`IsPlayingOrWillChangePlaymode`、需要的命令是否存在。

### 执行 deferred 命令

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Refresh"}'
```

CLI 会等待最终响应。若返回 `unity is compiling`，先轮询 `Ping`，等 `IsCompiling == false` 后重试。

### 查资源

先小范围查，不要全项目大结果：

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"AssetFindRequest","Filter":"t:Prefab","MaxResults":10}'
```

需要字段格式时查对应 proto；需要路径归一化或返回逻辑时查 `UnityBridgeAsset*Handler.cs`。

### 操作场景对象

先读层级或查对象，再改：

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"SceneGetHierarchyRequest","Depth":2,"IncludeInactive":false}'
```

写操作后用 `GameObjectGetInfoRequest` 或 `TransformGetRequest` 验证，不要只相信命令成功。

### 改 Inspector

先读组件：

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"InspectorGetComponentsRequest","Path":"<HierarchyPath>"}'
```

再读属性名和类型，最后使用 set 命令。不要猜 `SerializedProperty` 路径。

### 跑 Editor 测试

使用精确正则，避免全量跑：

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"UnityTestRunRequest","Name":"^Unitybridge_DeferredHandlerRunContext_Test$"}'
```

判定：`Error == 0`、`Matched > 0`、`Failed == 0`。

## 常见错误处理

- `wait unity bridge response timeout`：Unity 未打开、项目未加载、root 不一致，或 Editor 未处理请求。
- `unity is compiling`：等待编译结束再发写命令。
- `unity already in playmode or changing playmode`：不要重复 `EnterPlay`。
- `unity not in playmode`：`Reload` / `ExitPlay` 前置不满足。
- `handler is missing`：先 `HostState` 确认可用命令，再查 proto 名是否写错。
- `execute menu item failed`：菜单路径不存在或当前 Unity 状态不允许。

## 输出要求

- 向用户汇报目标、命令、关键结果，不贴完整大 JSON。
- 写操作后说明如何验证，必要时直接执行验证命令。
- 如果 UnityBridge 不可用，说明原因和下一步，不回退到 GUI 点击，除非用户确认。
