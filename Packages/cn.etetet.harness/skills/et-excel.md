# et-excel - ET Excel 入口

## 何时使用

- 通过 `Bin/ET.ExcelMcp.dll` 读写 Excel
- 处理单元格、区域、样式、公式、图表、工作表
- 做批量数据导入导出、筛选、排序、合并单元格
- 维护 Luban 配置表内容，但还没有进入导出阶段

## 不要加载

- 只是编译、跑测试、做 Unity 编辑器操作
- 只是执行 Luban 导出，不需要实际读写 Excel（用 `et-luban`）
- 只是讨论表结构，不需要实际读写 Excel

## 默认动作

1. 先 `cli list` 或 `cli help <工具名>`，不要硬背完整工具表。
2. 能批量就批量，优先 `excel_range` / `excel_data_operations`，不要逐格操作。
3. JSON 参数只传本次调用最小字段集；`pwsh` 中优先外层单引号。
4. 写操作优先使用绝对路径；覆盖原文件前先确认目标文件。
5. 改完表如果需要生成配置产物，再叠加 `et-luban`。

## 优先入口

- `dotnet ./Bin/ET.ExcelMcp.dll cli list`
- `dotnet ./Bin/ET.ExcelMcp.dll cli help <工具名>`
- `dotnet ./Bin/ET.ExcelMcp.dll cli <工具名> '<JSON参数>'`

## 按需补读

- `skills/references/et-excel-cli.md`：工具选择、`pwsh` JSON 传参、常用示例
