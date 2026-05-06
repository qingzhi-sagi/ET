# ET Excel 参考

## 快速分流

- 工具名不确定：先执行 `list` 或 `help <工具名>`。
- 文件与转换：`excel_file_operations`。
- 单格读写：`excel_cell`。
- 二维区域：`excel_range`。
- 批量数据、筛选、排序：`excel_data_operations`。
- 样式：`excel_style`。
- 公式：`excel_formula`。
- 图表：`excel_chart`。
- 工作表：`excel_sheet`。
- 合并单元格：`excel_merge_cells`。
- 行列处理：`excel_row_column`。

## 高频命令

```powershell
dotnet ./Bin/ET.ExcelMcp.dll cli list
dotnet ./Bin/ET.ExcelMcp.dll cli help excel_range
```

```powershell
dotnet ./Bin/ET.ExcelMcp.dll cli excel_sheet '{"operation":"list","path":"C:\\Temp\\demo.xlsx"}'
```

```powershell
dotnet ./Bin/ET.ExcelMcp.dll cli excel_range '{"operation":"write","path":"C:\\Temp\\demo.xlsx","range":"A1:B2","data":[["A","B"],["C","D"]]}'
```

```powershell
dotnet ./Bin/ET.ExcelMcp.dll cli excel_formula '{"operation":"add","path":"C:\\Temp\\demo.xlsx","cell":"C1","formula":"=SUM(A1:B1)"}'
```

## pwsh 传参

正确写法：外层单引号，内部双引号。

```powershell
dotnet ./Bin/ET.ExcelMcp.dll cli excel_range '{"operation":"get","path":"C:\\Temp\\demo.xlsx","range":"A1:B2"}'
```

错误写法：`pwsh` 不把 `\"` 当成通用转义。

```powershell
dotnet ./Bin/ET.ExcelMcp.dll cli excel_range "{\"operation\":\"get\"}"
```

`--%` 停止解析时，JSON 不加外层引号。

```powershell
dotnet ./Bin/ET.ExcelMcp.dll cli --% excel_range {"operation":"get","path":"C:\\Temp\\demo.xlsx","range":"A1:B2"}
```

复杂 JSON 优先用 `ConvertTo-Json -Compress`。

```powershell
$json = @{
    operation = "write"
    path = "C:\Temp\demo.xlsx"
    range = "A1:B2"
    data = @(@("A","B"), @("C","D"))
} | ConvertTo-Json -Compress

dotnet ./Bin/ET.ExcelMcp.dll cli excel_range $json
```

## 常见检查

- 工具名不确定时，先看 `help`。
- 能批量处理时，不要逐格写入。
- Windows 路径在 JSON 字符串内反斜杠需要双写。
- 如果问题其实是导出、编译或运行链路，转 `et-luban` 或 `et-build`。
