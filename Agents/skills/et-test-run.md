# et-test-run - 测试执行入口

## 何时使用

- 执行测试（全部 / 指定用例）
- 查看测试日志、分析失败原因
- 验证修改后的代码是否通过测试或回归测试

## 不要加载

- 只是编写测试用例（用 `et-test-write`）
- 只是编译项目（用 `et-build`）

## 默认动作

1. 先用 `et-build` 执行 `dotnet build ET.sln`，除非用户明确只要看日志或已经完成编译。
2. 运行测试前清理 `Logs/`，避免旧日志干扰。
3. 优先使用 WOW 现有测试入口：管道输入 `Test` 命令到 `dotnet ./Bin/ET.App.dll --SceneName=Test`。
4. 失败时先看控制台首个失败点，再看 `Logs/All.log`。
5. 测试成功后也检查 `Logs/All.log`，确认没有隐藏异常或错误日志。
6. 调试失败遵循先定位原因、再改代码；不要为了跑通测试随意修改正常业务逻辑。

## 优先入口

- 编译：`dotnet build ET.sln`
- 清理日志：`Remove-Item ./Logs -Recurse -Force -ErrorAction SilentlyContinue`
- 全量测试：`"Test" | dotnet ./Bin/ET.App.dll --SceneName=Test`
- 指定测试：`"Test --Name=CreateRobot" | dotnet ./Bin/ET.App.dll --SceneName=Test`
- 正则测试：`"Test --Name=Quest.*" | dotnet ./Bin/ET.App.dll --SceneName=Test`
- 查看日志：`Get-Content ./Logs/All.log -Tail 200`

## 按需补读

- `Agents/skills/references/et-test-guide.md`：测试执行格式、日志分析、常见失败原因
