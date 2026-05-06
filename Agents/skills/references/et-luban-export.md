# ET Luban 参考

## 默认原则

- 命令必须在项目根目录执行。
- 本项目 Luban 导出入口：`dotnet ./Bin/ET.ExcelExporter.dll`。
- 当前脚本统一走 `cs-code` 与 `cs-code-data`，产物是 C# 代码。
- 只改表内容时先用 `et-excel`；真正生成产物时再用 `et-luban`。

## 导出入口

```powershell
dotnet ./Bin/ET.ExcelExporter.dll
```

Unity 菜单入口：

- `ET/Excel/ExcelExporter`

## 导出流程

1. 确认本次任务是否修改了 `Packages/cn.etetet.*/Luban/**`、`__tables__.xlsx`、`__beans__.xlsx`、`__enums__.xlsx` 或 `Defines/`。
2. 在项目根目录执行 `dotnet ./Bin/ET.ExcelExporter.dll`。
3. 检查控制台是否出现导出开始信息和 `excelexporter ok!`。
4. 用 `git diff --stat` 检查生成范围。
5. 核对 `CodeMode/Model/**`、`CodeMode/Config/**`、聚合后的 `luban.conf` 是否符合预期。
6. 如果导出后还要编译、启动、发布，再叠加 `et-build`。

## 常见落点

- 表定义：`Packages/cn.etetet.*/Luban/**`
- 聚合配置：`luban.conf`
- 代码产物：`Packages/cn.etetet.*/CodeMode/Model/**`
- 配置产物：`Packages/cn.etetet.*/CodeMode/Config/**`

## 常见问题

- 把产物理解成 json/bin：当前脚本实际导出 C# 代码产物。
- `[ERROR] 源文件 luban.conf 不存在`：对应配置集合缺少基准 `luban.conf`，需要先补齐或初始化。
- 导出后 diff 很大：先确认是不是 `schemaFiles` 聚合刷新，不要误判成手工逻辑改动。
- 只想维护表内容：不要直接导出，先用 `et-excel`。
