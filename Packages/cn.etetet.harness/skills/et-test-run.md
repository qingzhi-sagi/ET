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
3. 服务端 / Hotfix 测试使用 WOW 现有测试入口：管道输入 `Test` 命令到 `dotnet ./Bin/ET.App.dll --SceneName=Test`。
4. Unity Editor 测试使用 UnityBridge 的 `UnityTestRunRequest`，按 `Name` 正则匹配测试类名执行。
5. Editor 测试不走 Unity Test Framework 的 `Filter` / `ExecutionSettings` / Test Runner；不要把这套入口和 Unity 官方测试入口混用。
6. 失败时先看控制台首个失败点；服务端测试再看 `Logs/All.log`，Editor 测试看 response 里的 `Results[].Message`。
7. 服务端测试成功后也检查 `Logs/All.log`，确认没有隐藏异常或错误日志。
8. 调试失败遵循先定位原因、再改代码；不要为了跑通测试随意修改正常业务逻辑。

## 优先入口

- 编译：`dotnet build ET.sln`
- 清理日志：`Remove-Item ./Logs -Recurse -Force -ErrorAction SilentlyContinue`
- 服务端全量测试：`"Test" | dotnet ./Bin/ET.App.dll --SceneName=Test`
- 服务端指定测试：`"Test --Name=CreateRobot" | dotnet ./Bin/ET.App.dll --SceneName=Test`
- 服务端正则测试：`"Test --Name=Quest.*" | dotnet ./Bin/ET.App.dll --SceneName=Test`
- Editor 宿主检查：`dotnet ./Bin/ET.UnityBridge.dll '{"_t":"Ping"}'`
- Editor 命令发现：`dotnet ./Bin/ET.UnityBridge.dll '{"_t":"HostState"}'`
- Editor 全量测试：`dotnet ./Bin/ET.UnityBridge.dll '{"_t":"UnityTestRunRequest","Name":".*"}'`
- Editor 指定测试：`dotnet ./Bin/ET.UnityBridge.dll '{"_t":"UnityTestRunRequest","Name":"^Unitybridge_UnityTestRunHandler_Test$"}'`
- Editor 正则测试：`dotnet ./Bin/ET.UnityBridge.dll '{"_t":"UnityTestRunRequest","Name":"^Unitybridge_.*Message_Test$"}'`
- 查看日志：`Get-Content ./Logs/All.log -Tail 200`

## Editor 测试结果判定

- 成功：response `Error == 0`、`Matched > 0`、`Failed == 0`、`Passed == Matched`。
- 未匹配或执行失败：response `Error != 0`，优先看 `Message` 和 `Results[].Message`。
- 找不到 `UnityTestRunRequest`：先确认 Unity Editor 已打开并加载项目，再执行 `Refresh` 或重新编译。
- Editor 测试类应继承 `ET.Test.ATestHandler`，通常放在目标包的 `Scripts/Editor/Test/` 下。

## 按需补读

- `skills/references/et-test-guide.md`：测试执行格式、日志分析、常见失败原因
