# et-build - ET Build 入口

## 何时使用

- 编译项目（`dotnet build ET.sln`）
- 导出 Proto 文件（.proto → C#）
- 启动服务器
- 发布 Linux 版本

## 不要加载

- 只是改代码、写测试，还没到编译或导出环节
- 只是导出 Luban 配置，此时改用 `et-luban`
- 只是查询架构规范或包依赖

## 默认动作

1. 所有命令必须在 **`pwsh`（PowerShell 7）** 中执行，不要混用 Windows 自带的 `powershell.exe`。
2. 唯一编译命令：`dotnet build ET.sln`，无论任何场景都用这个。
3. 运行服务器前先删除 `Logs/` 目录，方便排查问题。
4. 服务器必须在 **Unity 根目录**下启动，不是 `Bin/` 目录。
5. Model / Hotfix 程序集不能用 IDE 编译，必须用 Unity 编辑器（F6）。

## 命令速查

### 编译

```powershell
dotnet build ET.sln
```

### Proto 文件导出

```powershell
dotnet Bin/ET.Proto2CS.dll
```

- Proto 生成文件落在 proto 包中
- Proto 文件名带的编号是唯一的，是 100 的倍数

### 服务器启动

```powershell
dotnet Bin/ET.App.dll --Console=1
```

- 需要管理员权限
- 在 Unity 根目录下执行

### 发布 Linux 版本

```powershell
pwsh -ExecutionPolicy Bypass -File Scripts/Publish.ps1
```

## 注意事项

- 需要全局代理才能下载 NuGet 包
- 分析器编译也必须使用 `ET.sln`，不能单独编译包
- Luban 配置导出已经独立到 `et-luban`，不要继续混在本 skill 里执行

## 常见问题

- **编译报错**：确认用的是 `dotnet build ET.sln`，不是单独编译某个包
- **启动失败**：检查是否有管理员权限、运行目录是否在 Unity 根目录
- **找不到日志**：检查是否在 `Logs/` 目录，启动前记得先删旧日志
