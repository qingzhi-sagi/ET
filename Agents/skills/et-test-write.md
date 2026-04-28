# et-test-write - 测试编写入口

## 何时使用

- 编写新的测试用例
- 修改现有测试用例

## 不要加载

- 只是执行测试（用 `et-test-run`）
- 只是做 TDD 流程编排（用 `et-tdd`）

## 规范速查

### 测试落点

- **测试必须优先放在被测功能自己的 package 内**，不要把所有测试都集中放到 `cn.etetet.test`
- 测试文件位置固定为：`Packages/cn.etetet.{被测包}/Scripts/Hotfix/Test/`
- `cn.etetet.test` 只作为测试框架、测试运行入口、公共测试基础设施或真正跨包集成测试的承载包
- 如果测试只验证 `cn.etetet.map` 的逻辑，就放在 `cn.etetet.map`；只验证 `cn.etetet.conditionexpr`，就放在 `cn.etetet.conditionexpr`
- 跨多个业务包的端到端测试，才允许放到更高层聚合包或 `cn.etetet.test`

### 命名（分析器 ET0036 强制）

格式：`{PackageType}_{TestName}_Test`
- `{PackageType}` 必须与所在包的 `PackageType` 常量匹配
- 必须以 `_Test` 结尾
- 文件必须放在 `Scripts/Hotfix/Test/` 目录下

```
cn.etetet.robot          → Robot_LoginFlow_Test
cn.etetet.map            → Map_AOI_Test
cn.etetet.conditionexpr  → ConditionExpr_Parse_Test
cn.etetet.test           → Test_CrossPackageFlow_Test
```

### 返回值

- 成功：`return ErrorCode.ERR_Success;`（即 0）
- 失败：直接返回数字 `return 1;`、`return 2;` …
- **每个失败分支的返回值必须唯一**，不要在正式包的 ErrorCode.cs 里定义测试专用常量

### 日志

- 测试失败必须用 `Log.Console`（不要用 `Log.Error`）
- 普通信息用 `Log.Debug`
- 统一使用英文

### 测试数据

- **不要修改已有的 json 或 excel 配置**
- 在代码里直接写 JSON 字符串构造配置数据

### 组件初始化

- 哪些组件由 `FiberInit_*` 提供、哪些由测试方法创建 — 在 Arrange 阶段先想清楚
- 组件应存在时直接用，不要加防御式判空；当前方法负责初始化时直接 `AddComponent<T>()`
- 详细规则参考 `et-code.md` 的"关键规范 / 结构与契约"

## 测试模板

```csharp
// 位置：Packages/cn.etetet.{被测包}/Scripts/Hotfix/Test/{PackageType}_{名称}_Test.cs
namespace ET
{
    [Test]
    public class Map_AOI_Test : ATestHandler
    {
        protected override async ETTask<int> Run(Fiber fiber, TestArgs args)
        {
            // Arrange
            EntityRef<SomeComponent> compRef = fiber.Root.GetComponent<SomeComponent>();

            // Act
            await SomeAsyncOperation();

            // Assert - await 后必须重新获取
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

## 常见错误

- 把功能包自己的测试写到 `cn.etetet.test` 中，导致测试和被测代码分离
- 测试类名前缀与所在包 `PackageType` 不一致
- `await` 后直接访问旧 Entity 变量（应通过 `EntityRef` 重新获取）
- 不同失败分支返回了相同的错误码
- 用 `Log.Error` 代替 `Log.Console` 输出测试错误
- 修改了已有 json/excel 配置文件来准备测试数据
- 在正式包的 ErrorCode.cs 里定义了只用于测试的错误码

## 完成后

1. 编译：`et-build`
2. 执行：`et-test-run`
