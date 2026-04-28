# et-test-run - 测试执行入口

## 何时使用

- 执行测试（全部 / 指定用例）
- 查看测试日志、分析失败原因
- 验证修改后的代码是否通过测试

## 不要加载

- 只是编写测试用例（用 `et-test-write`）
- 只是编译项目（用 `et-build`）

## 执行流程

1. 先用 `et-build` 执行 `dotnet build ET.sln`
2. 删除 `Logs/`
3. 启动测试场景并发送测试命令
4. 先看控制台，再看 `Logs/All.log`

## 命令速查

### 编译（必须 Debug 模式）

```powershell
dotnet build ET.sln
```

### 删除旧日志

```powershell
Remove-Item ./Logs -Recurse -Force -ErrorAction SilentlyContinue
```

### 单命令执行（推荐）

```powershell
# 执行所有测试
"Test" | dotnet ./Bin/ET.App.dll --SceneName=Test

# 执行指定测试（支持正则）
"Test --Name=CreateRobot" | dotnet ./Bin/ET.App.dll --SceneName=Test
"Test --Name=Quest.*" | dotnet ./Bin/ET.App.dll --SceneName=Test
```

### 交互式执行

```powershell
dotnet ./Bin/ET.App.dll --SceneName=Test
# 等待 > 提示符后输入：
# Test
# Test --Name=CreateRobot
# Test --Name=Quest.*
```

## 重要提醒

- **运行前必须删除 `Logs/` 目录**，否则旧日志会干扰排查
- 测试用例可以分布在各自功能包的 `Scripts/Hotfix/Test/` 下，不要默认认为都在 `cn.etetet.test`
- 按名称过滤时优先使用被测包的 `PackageType` 前缀，例如 `ConditionExpr`、`Map`、`Robot`

## 日志分析

- 日志位置：`Logs/All.log`
- 失败时先看控制台输出，再查 `All.log`

### 常见失败原因

| 现象 | 原因 |
|---|---|
| Entity 已失效 | `await` 后未通过 `EntityRef` 重新获取 |
| 数据不一致 | 测试数据准备逻辑有误 |
| 消息超时 | 网络消息发送有误 |
| Fiber 未找到 | 场景或 Fiber 名称错误 |

## 结果格式

```
Test.Test_CreateRobot_Test start
Test.Test_CreateRobot_Test success   ← 成功
ConditionExpr.ConditionExpr_Parse_Test success

Test.Test_XXX_Test fail              ← 失败，后面跟错误详情
not found test! package: .* name: X ← 未找到匹配用例
```
