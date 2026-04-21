# et-luban - Luban 导出专家

这个 skill 负责导出 Luban 生成的 C# 配置代码与 C# 数据代码，并排查 `ET.ExcelExporter`、`luban.conf`、`LubanGen.ps1` 相关问题。

## 何时使用

- 导出 Luban 生成的 C# 配置代码与 C# 数据代码
- 修改 `Packages/cn.etetet.*/Luban/**` 下的表、`__tables__.xlsx`、`__beans__.xlsx`、`__enums__.xlsx`、`Defines/` 后需要重新导出
- 刷新聚合后的 `luban.conf`
- 排查 `ET.ExcelExporter`、`LubanGen.ps1`、`luban.conf` 导出失败
- 核对导出后的 `CodeMode/Model/**` 与 `CodeMode/Config/**` 结果

## 不要加载

- 只是读取/写入 Excel 单元格、批量改表，还没到导出环节
- 只是编译 `ET.sln`、导出 Proto、启动服务器、发布
- 只是查询 Unity 编辑器状态或执行其它 UnityBridge 命令

## 默认动作

1. 所有命令使用 **`pwsh`（PowerShell 7）**，不要混用 Windows 自带的 `powershell.exe`。
2. 命令必须在 **项目根目录** 执行，避免 `Packages/` 相对路径和 `LubanGen.ps1` 定位错误。
3. 当前 Luban 脚本实际统一走 `-c cs-code -d cs-code-data`，导出结果是 C# 代码，不是 json 或二进制。
4. 导出前先区分本次任务是“改表”还是“生成产物”；只改表优先 `et-excel`，真正导出时再叠加本 skill。
5. 导出过程会扫描 `Packages/cn.etetet.*/Luban/*`，汇总对应集合的 `schemaFiles`，并重写基准 `luban.conf` 后再执行生成脚本。
6. 常见落点是 `Packages/cn.etetet.*/CodeMode/Model/**` 和 `Packages/cn.etetet.*/CodeMode/Config/**`。

## 命令速查

### 导出配置代码

```powershell
dotnet ./Bin/ET.ExcelExporter.dll
```

### Unity 菜单入口

- `ET/Excel/ExcelExporter`

## 工作流程

1. 先确认是不是只改 Excel；如果只是改表结构或单元格，先加载 `et-excel`
2. 在项目根目录用 `pwsh` 执行 `ET.ExcelExporter`
3. 检查控制台是否出现 `[INFO] 开始导出 ...` 和 `excelexporter ok!`
4. 必要时检查生成结果与 diff，区分真实表改动、聚合后的 `luban.conf` 变化、`CodeMode/Model` / `CodeMode/Config` 生成结果变化
5. 如果导出后还要编译、启动、发布，再叠加 `et-build`

## 常见问题

- **把产物理解成 json/bin**：当前脚本实际使用 `cs-code` 与 `cs-code-data`，应按 C# 代码产物理解
- **`[ERROR] 源文件 luban.conf 不存在`**：对应配置集合缺少基准 `luban.conf`，需要先补齐或初始化
- **导出后 diff 很大**：先确认是不是 `schemaFiles` 聚合刷新导致，不要把自动聚合变化误判成手工逻辑改动
- **只想维护表内容**：不要直接加载本 skill，改用 `et-excel`
