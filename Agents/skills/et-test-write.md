# et-test-write - 测试编写入口

## 何时使用

- 编写新的 `ATestHandler` 测试用例
- 修改现有测试用例
- 补 `Test.md` 或最小验证清单

## 不要加载

- 只是执行测试或查看日志（用 `et-test-run`）
- 只是做完整 TDD 流程编排（用 `et-tdd`）

## 默认动作

1. 测试优先放在被测功能自己的 package 内：`Packages/cn.etetet.{被测包}/Scripts/Hotfix/Test/`。
2. `cn.etetet.test` 只承载测试框架、公共测试基础设施，以及测试 test 包自身代码的测试；跨包集成测试也不能放到 test 包。
3. 跨包集成测试必须放到拥有该集成场景的业务包；若确实是多业务编排，放到发起该编排的更高层业务包。
4. 类名遵守 `{PackageType}_{TestName}_Test`，并与所在包 `PackageType` 常量匹配。
5. 测试必须是协程式、逻辑确定的验证：按真实时序 `await` 消费事件或结果，绝对禁止用时间等待某种状态出现。
6. ObjectWait 必须按真实时序逐个消费事件，禁止睡眠轮询、`WaitMatch` 或无序过滤跳过事件。
7. 含 `await` 的测试必须在 `await` 后通过 `EntityRef` 重新获取 Entity。
8. 测试需要的 Config 数据必须由测试自己用代码构造；当前 Excel 配置已导出成代码，不能依赖项目已有配置产物，也不要修改已有配置文件或导出的配置代码。
9. 成功返回 `ErrorCode.ERR_Success`；失败直接返回唯一数字错误码，不污染正式 `ErrorCode.cs`。
10. 测试失败用 `Log.Console`，普通信息用 `Log.Debug`，日志统一英文。

## 快速分流

- 命名、落点、模板、ObjectWait、数据准备：补读 `Agents/skills/references/et-test-guide.md`
- 异步安全：叠加 `et-async`
- 写完后编译：转 `et-build`
- 写完后执行：转 `et-test-run`
