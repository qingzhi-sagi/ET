# et-test-write - 测试编写入口

## 何时使用

- 编写新的 `ATestHandler` 测试用例
- 修改现有测试用例
- 补 `Test.md` 或最小验证清单

## 不要加载

- 只是执行测试或查看日志（用 `et-test-run`）
- 只是做完整 TDD 流程编排（用 `et-tdd`）

## 默认动作

以下规则是硬性约束，不是建议。新增或修改 `ATestHandler` 时，必须先按本 skill 判断测试落点和类名；不得把测试临时放到 `cn.etetet.test`，不得用“那里已有测试”或“只是协议/公共测试”作为例外理由。

1. 测试优先放在被测功能自己的 package 内：`Packages/cn.etetet.{被测包}/Scripts/Hotfix/Test/`。
2. 测试专用的数据结构、Entity、Component、BT 节点或辅助模型类型必须放在被测包的 `Scripts/Model/Test/`；不得放到 `Scripts/Model/Share/` 或 `Scripts/Hotfix/Test/`。
3. 移动测试专用 C# 文件时同步移动 `.meta`，并通过 UnityBridge `RegenProject` 或项目既有流程刷新工程引用。
4. `cn.etetet.test` 只承载测试框架、公共测试基础设施，以及测试 test 包自身代码的测试；跨包集成测试也不能放到 test 包。
5. 跨包集成测试必须放到拥有该集成场景的业务包；若确实是多业务编排，放到发起该编排的更高层业务包。
6. 类名遵守 `{PackageType}_{TestName}_Test`，并与所在包 `PackageType` 常量匹配。
7. 测试必须是协程式、逻辑确定的验证：按真实时序 `await` 消费事件或结果，绝对禁止用时间等待某种状态出现。
8. ObjectWait 必须按真实时序逐个消费事件，禁止睡眠轮询、`WaitMatch` 或无序过滤跳过事件。
9. 含 `await` 的测试必须在 `await` 后通过 `EntityRef` 重新获取 Entity。
10. 测试需要的 Config 数据必须由测试自己用代码构造；当前 Excel 配置已导出成代码，不能依赖项目已有配置产物，也不要修改已有配置文件或导出的配置代码。
11. 成功返回 `ErrorCode.ERR_Success`；失败直接返回唯一数字错误码，不污染正式 `ErrorCode.cs`。
12. 测试失败用 `Log.Console`，普通信息用 `Log.Debug`，日志统一英文。

## 快速分流

- 命名、落点、模板、ObjectWait、数据准备：补读 `skills/references/et-test-guide.md`
- 异步安全：叠加 `et-async`
- 写完后编译：转 `et-build`
- 写完后执行：转 `et-test-run`
