# ET Test 参考

## 快速分流

- TDD 或测试方案：看“TDD 流程”。
- 写或改 `ATestHandler`：看“测试编写”。
- 只跑服务端 / Hotfix 测试：看“服务端测试执行”。
- 只跑 Unity Editor 测试：看“Editor 测试执行”。
- 排查失败：看“日志分析”和“常见失败原因”。

## TDD 流程

1. 先理解需求、包内 `AGENTS.md`、现有实现与现有测试。
2. 写代码前先补 `Test.md` 或最小验证清单，明确协程式、逻辑确定的验证路径。
3. 写最小可跑用例，先让目标测试失败。
4. 实现代码到目标测试通过。
5. 编译统一使用 `dotnet build ET.sln`。
6. 跑目标测试，再回归相邻测试。
7. 测试成功后也检查 `Logs/All.log`，确认没有隐藏异常或错误日志。
8. 完成后更新包内 `AGENTS.md`，说明实现原理、使用方式、测试入口或本次沉淀的新规则。

## 测试编写

### 命名与落点

- 测试优先放在被测功能自己的 package 内。
- 测试用例入口目录：`Packages/cn.etetet.{被测包}/Scripts/Hotfix/Test/`。
- 测试专用的数据结构、Entity、Component、BT 节点或辅助模型类型必须放在被测包的 `Scripts/Model/Test/`。
- `Scripts/Model/Share/` 只放运行时或配置编译需要的公共模型，不放只为测试声明的类型。
- `Scripts/Hotfix/Test/` 只放测试入口和测试流程代码，不放测试专用模型数据结构。
- 移动测试专用 C# 文件时同步移动 `.meta`，并通过 UnityBridge 或项目既有流程刷新工程引用。
- `cn.etetet.test` 只承载测试框架、公共测试基础设施，以及测试 test 包自身代码的测试；跨包集成测试也不能放到 test 包。
- 跨包集成测试必须放到拥有该集成场景的业务包；若确实是多业务编排，放到发起该编排的更高层业务包。
- 类名：`{PackageType}_{TestName}_Test`。
- 父类：`ATestHandler`。
- 入口：`protected override async ETTask<int> Run(Fiber fiber, TestArgs args)`。
- 需要 UnityEditor API 的 Editor 测试放在目标包的 `Scripts/Editor/Test/` 下，通常继承 `ET.Test.ATestHandler`，并通过 UnityBridge 的 `UnityTestRunRequest` 执行。
- Editor 测试不是 Unity Test Framework 测试，不使用 Unity 官方 Test Runner、`Filter` 或 `ExecutionSettings`。

### 返回值与日志

- 成功：`return ErrorCode.ERR_Success;`。
- 失败：直接返回唯一数字错误码，如 `return 1;`、`return 2;`。
- 不要在正式包 `ErrorCode.cs` 中定义测试专用错误码。
- 测试失败用 `Log.Console`。
- 普通信息用 `Log.Debug`。
- 日志统一英文。
- 成功后也检查 `Logs/All.log`。

### ObjectWait 规则

- 测试必须是协程式、逻辑确定的验证；用事件、消息返回或明确完成信号来推进断言。
- 组件状态只能在确定触发链完成后同步断言，不能通过等待时间或轮询来观察状态何时出现。
- 绝对禁止使用时间等待某种状态出现，例如 `TimerComponent.WaitAsync(...)`、固定延迟、sleep 或轮询状态直到超时。
- 全部使用 `await` 消费真实事件或异步结果，不要睡眠轮询。
- 固定时间等待只允许作为超时保护，不能作为业务完成判断，也不能作为“等状态刷出来”的手段。
- 不要使用 `WaitMatch` 或无序过滤跳过事件。
- 等待按真实时序逐个消费。
- 事件桥接器只做转发，不写业务过滤。
- 等待任务按需创建；只有防丢事件时才先注册再触发。
- 推荐“客户端期望协程 + 服务端期望协程 + 主协程”。

### 数据准备

- 不要修改已有配置文件、Excel 表或导出的配置代码来准备测试数据。
- 测试需要的 Config 数据必须由测试自己用代码构造；当前 Excel 配置已导出成代码，不能依赖项目已有配置产物。
- 如果测试可能读到已有配置，优先在测试前清理或隔离，确保数据只来自本测试。
- 如果测试失败发现配置不存在，可以从 git 历史确认字段，再构造最小数据。
- 每个测试用例运行在全新独立环境，通常不需要清理测试数据。

### 异步与 Entity

- 含 `await` 的测试必须在 `await` 后通过 `EntityRef` 重新获取 Entity。
- 需要服务端数据时，通过 `Fiber` 与组件重新获取，不要复用旧 Entity。
- 组件应存在时直接用；当前方法负责初始化时直接 `AddComponent<T>()`。

## 测试模板

```csharp
namespace ET
{
    [Test]
    public class Map_AOI_Test : ATestHandler
    {
        protected override async ETTask<int> Run(Fiber fiber, TestArgs args)
        {
            EntityRef<SomeComponent> compRef = fiber.Root.GetComponent<SomeComponent>();

            await SomeAsyncOperation();

            SomeComponent comp = compRef;
            if (comp == null)
            {
                Log.Console("comp is null");
                return 1;
            }

            if (comp.Value != expectedValue)
            {
                Log.Console($"value error, expected: {expectedValue}, actual: {comp.Value}");
                return 2;
            }

            return ErrorCode.ERR_Success;
        }
    }
}
```

## 服务端测试执行

```powershell
dotnet build ET.sln
```

```powershell
Remove-Item ./Logs -Recurse -Force -ErrorAction SilentlyContinue
```

```powershell
"Test" | dotnet ./Bin/ET.App.dll --SceneName=Test
```

```powershell
"Test --Name=CreateRobot" | dotnet ./Bin/ET.App.dll --SceneName=Test
```

```powershell
"Test --Name=Quest.*" | dotnet ./Bin/ET.App.dll --SceneName=Test
```

交互式：

```powershell
dotnet ./Bin/ET.App.dll --SceneName=Test
```

进入提示符后输入：

```text
Test
Test --Name=CreateRobot
Test --Name=Quest.*
```

## Editor 测试执行

Editor 测试用于验证依赖 UnityEditor / UnityEngine 编辑器上下文的代码。执行前必须打开 Unity Editor，并确认项目已加载、UnityBridge 宿主在线。

```powershell
dotnet build ET.sln
```

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Ping"}'
```

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"HostState"}'
```

全量执行：

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"UnityTestRunRequest","Name":".*"}'
```

指定测试：

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"UnityTestRunRequest","Name":"^Unitybridge_UnityTestRunHandler_Test$"}'
```

正则执行：

```powershell
dotnet ./Bin/ET.UnityBridge.dll '{"_t":"UnityTestRunRequest","Name":"^Unitybridge_.*Message_Test$"}'
```

结果判定：

- 成功：response `Error == 0`、`Matched > 0`、`Failed == 0`、`Passed == Matched`。
- 失败：response `Error != 0`，优先看 `Message` 和 `Results[].Message`。
- `Matched == 0` 代表正则没有匹配到任何 Editor 测试类。
- 找不到 `UnityTestRunRequest` 时，先确认 `HostState` 是否包含该命令；必要时执行 `dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Refresh"}'` 后重试。

## 日志分析

- 日志位置：`Logs/All.log`。
- 失败时先看控制台输出，再查 `All.log`。
- 查看尾部日志：`Get-Content ./Logs/All.log -Tail 200`。
- 成功格式：`Package.TestName success`。
- 失败格式：`Package.TestName fail`，后面跟错误详情。
- 未找到匹配：`not found test! package: .* name: X`。
- 即使控制台显示 success，也要检查 `Logs/All.log` 是否存在隐藏异常、错误日志或异步尾部报错。
- Editor 测试结果以 UnityBridge response 为准，不依赖 `Logs/All.log`；失败详情优先看 `Message` 和 `Results[].Message`。

## 常见失败原因

| 现象 | 优先检查 |
|---|---|
| Entity 已失效 | `await` 后是否通过 `EntityRef` 重新获取 |
| 数据不一致 | 测试准备与配置链路 |
| 消息超时 | 网络消息发送、事件是否真的发布 |
| Fiber 未找到 | 场景或 Fiber 名称 |
| 未找到测试 | 类名、`PackageType` 前缀、测试目录 |
| Editor 测试未匹配 | `Name` 正则、类是否继承 `ET.Test.ATestHandler`、是否在 `Scripts/Editor/Test/` |
| 找不到 `UnityTestRunRequest` | Unity Editor 是否打开、UnityBridge 是否在线、是否执行过 `Refresh` |
| 配置不存在 | 测试是否自己用代码构造了最小 Config 数据 |

## 常见错误

- 把功能包自己的测试或跨包集成测试写到 `cn.etetet.test`。
- 把测试专用数据结构、Entity、Component、BT 节点或辅助模型类型放到 `Scripts/Model/Share/` 或 `Scripts/Hotfix/Test/`。
- 测试类名前缀与所在包 `PackageType` 不一致。
- `await` 后直接访问旧 Entity。
- 不同失败分支返回相同错误码。
- 用 `Log.Error` 代替 `Log.Console`。
- 修改已有配置文件、Excel 表或导出的配置代码准备测试数据。
- 依赖项目已有配置产物而不是在测试内用代码构造 Config 数据。
- 使用固定时间等待某种状态出现。
- 使用睡眠轮询、`WaitMatch` 或无序过滤跳过事件。
- 在正式包 `ErrorCode.cs` 里定义测试专用错误码。
- 为了跑通测试随意修改正常业务逻辑。
- 用 Unity Test Framework 的 `Filter` / `ExecutionSettings` / Test Runner 跑 UnityBridge Editor 测试。
