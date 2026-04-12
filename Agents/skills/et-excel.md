# et-excel - Excel 操作专家

这个 skill 负责通过 `ET.ExcelMcp` 的 CLI 入口读写 Excel、维护 Luban 配置表、做批量表格处理。

## 何时使用

- 创建/读取 Excel 文件
- 单元格读写操作
- 批量数据导入导出
- 设置单元格样式和格式
- 添加图表、公式
- 数据筛选、排序
- 合并单元格
- 添加数据验证

## 不要加载

- 只是编译、跑测试、做 Unity 编辑器操作
- 只是讨论表结构，不需要实际读写 Excel

## 默认动作

1. 所有命令使用 **`pwsh`（PowerShell 7）**，不要混用 Windows 自带的 `powershell.exe`。
2. 先用 `cli list` 看工具，再用 `cli help <工具名>` 确认参数，不要硬背全部工具。
3. JSON 参数用单引号包裹，内部统一双引号。
4. 写操作优先使用绝对路径；覆盖原文件前先确认目标文件。
5. Luban 表操作优先先读再写，避免误覆盖。

## 命令行调用格式

```powershell
dotnet ./Bin/ET.ExcelMcp.dll cli <工具名> '<JSON参数>'
```

### 查看所有可用工具

```powershell
dotnet ./Bin/ET.ExcelMcp.dll cli list
```

### 查看某个工具的详细帮助

```powershell
dotnet ./Bin/ET.ExcelMcp.dll cli help <工具名>
```

## 常用入口

### 先做工具发现

```powershell
dotnet ./Bin/ET.ExcelMcp.dll cli list
dotnet ./Bin/ET.ExcelMcp.dll cli help excel_cell
```

### 文件操作 (`excel_file_operations`)

```powershell
# 创建新 Excel 文件
dotnet ./Bin/ET.ExcelMcp.dll cli excel_file_operations '{"operation":"create","path":"C:/Temp/output.xlsx"}'

# 转换为 CSV
dotnet ./Bin/ET.ExcelMcp.dll cli excel_file_operations '{"operation":"convert","inputPath":"C:/Temp/input.xlsx","outputPath":"C:/Temp/output.csv","format":"csv"}'
```

### 单元格操作 (`excel_cell`)

```powershell
# 写入单元格
dotnet ./Bin/ET.ExcelMcp.dll cli excel_cell '{"operation":"write","path":"C:/Temp/test.xlsx","cell":"A1","value":"Hello"}'

# 读取单元格
dotnet ./Bin/ET.ExcelMcp.dll cli excel_cell '{"operation":"get","path":"C:/Temp/test.xlsx","cell":"A1"}'
```

### 范围与批量数据 (`excel_range` / `excel_data_operations`)

```powershell
# 批量写入二维数据
dotnet ./Bin/ET.ExcelMcp.dll cli excel_range '{"operation":"write","path":"C:/Temp/test.xlsx","range":"A1:B2","data":[["姓名","年龄"],["Alice",18]]}'

# 读取范围内容
dotnet ./Bin/ET.ExcelMcp.dll cli excel_data_operations '{"operation":"get_content","path":"C:/Temp/test.xlsx","range":"A1:C10"}'
```

### 工作表操作 (`excel_sheet`)

```powershell
# 创建工作表
dotnet ./Bin/ET.ExcelMcp.dll cli excel_sheet '{"operation":"create","path":"C:/Temp/test.xlsx","sheetName":"Config"}'

# 列出工作表
dotnet ./Bin/ET.ExcelMcp.dll cli excel_sheet '{"operation":"list","path":"C:/Temp/test.xlsx"}'
```

### 样式与公式 (`excel_style` / `excel_formula`)

```powershell
# 添加公式
dotnet ./Bin/ET.ExcelMcp.dll cli excel_formula '{"operation":"add","path":"C:/Temp/test.xlsx","cell":"B5","formula":"=SUM(B1:B4)"}'

# 设置标题样式
dotnet ./Bin/ET.ExcelMcp.dll cli excel_style '{"operation":"format","path":"C:/Temp/test.xlsx","range":"A1:C1","bold":true,"backgroundColor":"#4472C4","fontColor":"#FFFFFF"}'
```

## 常用工具速查

- `excel_file_operations`：创建、转换文件
- `excel_cell`：单元格读写
- `excel_range`：二维范围写入、复制
- `excel_data_operations`：批量数据、排序、读取范围内容
- `excel_sheet`：工作表管理
- `excel_style`：样式格式化
- `excel_formula`：公式处理
- `excel_merge_cells`：合并/取消合并
- `excel_row_column`：行列插入、尺寸调整
- `excel_chart`：图表

其余工具以 `dotnet ./Bin/ET.ExcelMcp.dll cli list` 实时查询，避免文档过时。

## 注意事项

1. **路径**：优先绝对路径，避免 Unity 根目录切换导致读错文件
2. **JSON 转义**：在 `pwsh` 中用单引号包裹 JSON，内部用双引号
3. **中文支持**：完全支持中文内容读写
4. **sheetIndex**：工作表索引从 0 开始
5. **outputPath**：写操作可指定输出路径，默认覆盖原文件

## 工作流程

当用户需要操作 Excel 时：

1. 确认操作类型（读/写/格式化/图表等）
2. 先 `cli list` / `cli help` 确认工具和参数
3. 构建 JSON 参数
4. 执行 CLI 命令
5. 检查输出结果
