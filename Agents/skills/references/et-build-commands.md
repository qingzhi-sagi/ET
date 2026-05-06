# ET Build 参考

## 默认原则

- 唯一编译命令：`dotnet build ET.sln`。
- 分析器编译统一使用 `ET.sln`，不要换成局部方案或 IDE 私有编译方式。
- 能调用项目现有脚本或 CLI 就不要手工拼长命令。
- 当前工作目录应位于 Unity 项目根目录。

## 高频命令

```powershell
dotnet build ET.sln
```

```powershell
dotnet ./Bin/ET.Proto2CS.dll
```

```powershell
Remove-Item ./Logs -Recurse -Force -ErrorAction SilentlyContinue
dotnet ./Bin/ET.App.dll --Console=1
```

```powershell
pwsh -ExecutionPolicy Bypass -File ./Scripts/Publish.ps1
```

## Proto 导出

- Proto 生成文件落在 proto 包中。
- Proto 文件名带的编号必须唯一，通常是 100 的倍数。
- 导出后必要时用 `git diff --stat` 确认生成范围。

## 服务器启动

- 需要管理员权限。
- 必须在 Unity 项目根目录执行，不在 `Bin/` 下执行。
- 启动前清理旧 `Logs/`，避免日志混淆。

## 发布

- Linux 发布入口：`pwsh -ExecutionPolicy Bypass -File ./Scripts/Publish.ps1`。
- 发布前先完成必要编译、配置导出与测试验证。

## 常见排查

- 编译报错：确认使用的是 `dotnet build ET.sln`。
- 分析器结果异常：确认没有单独编译某个包或用 IDE 私有编译。
- NuGet 还原失败：检查网络和代理。
- 启动失败：检查管理员权限、运行目录、端口占用。
- 找不到日志：确认 `Logs/` 是否生成最新文件。
