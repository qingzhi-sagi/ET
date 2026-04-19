# ET.ExcelMcp (EPPlus)

`ET.ExcelMcp` 位于 WOW 仓库 `Packages/cn.etetet.config/DotNet~/ET.ExcelMcp` 目录，是一个基于 EPPlus 的 Excel 专用 MCP (Model Context Protocol) 服务器。该工程整体能力移植自 `../aspose-mcp-server`，并对 EPPlus 做了适配，以便在不依赖商业组件的情况下在 ET 框架内默认启用。

## 项目背景

- 🧬 **移植来源**：沿用 aspose 版本的工具设计、协议与测试集合，只是底层由 Aspose.Cells 更换为 EPPlus 7.5.2。
- 🧱 **工程形态**：通过 `.csproj` 直接集成在 ET Package 体系中，目标框架为 `net10.0`。
- 🛡️ **安全收敛**：提供 `SecurityHelper`、`ArgumentHelper` 等辅助类，约束输入路径、数组尺寸与字符串长度，防止上下文溢出。

## 快速开始

> ⚠️ 所有命令请在 PowerShell 中执行，默认当前工作目录为 WOW 仓库根目录。

```powershell
$project = "Packages/cn.etetet.config/DotNet~/ET.ExcelMcp/ET.ExcelMcp.csproj"
$serverDll = Join-Path $PWD "Bin/ET.ExcelMcp.dll"

# 推荐直接构建 ET.sln，生成 WOW/Bin/ET.ExcelMcp.dll
dotnet build ET.sln

# 若仅需局部验证，也可单独构建
dotnet build $project

# 必须通过 Bin 目录下的成品 DLL 启动 MCP
dotnet $serverDll
```

启动成功后，控制台会输出 `EPPlus MCP Server - Excel专用服务器`，并等待 MCP 客户端通过 STDIN/STDOUT 交互。

## 功能特性

- ✅ **文件操作**: 创建Excel工作簿、格式转换 (CSV)
- ✅ **工作表管理**: 创建、删除、重命名、列出工作表
- ✅ **单元格操作**: 读取、写入、编辑、清空单元格
- ✅ **公式支持**: 支持Excel公式的设置和读取
- ✅ **类型自动识别**: 自动识别并转换数字、布尔值、日期等类型
- ✅ **中文支持**: 全面支持中文字符
- ✅ **高级功能**: 数据透视表、条件格式等增强功能

## 系统要求

- .NET 10.0 或更高版本
- EPPlus 7.5.2

## 构建和运行

```powershell
$project = "Packages/cn.etetet.config/DotNet~/ET.ExcelMcp/ET.ExcelMcp.csproj"
$serverDll = Join-Path $PWD "Bin/ET.ExcelMcp.dll"

# 统一构建
dotnet build ET.sln

# 或单独构建
dotnet build $project

# 始终通过 Bin 中的 DLL 启动
dotnet $serverDll
```

> 注意：运行 `dotnet run --project ...` 仅适合开发调试。正式启动请确保使用 `Bin/ET.ExcelMcp.dll`，避免重复编译开销并与 Claude/IDE 配置保持一致。

## MCP 客户端配置与使用

### 1. Claude Desktop

1. 打开 `claude_desktop_config.json`（macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`，Windows: `%APPDATA%/Claude/claude_desktop_config.json`）。
2. 在 `mcpServers` 中新增条目，命令使用 PowerShell，以保证与仓库脚手架一致：

```json
{
  "mcpServers": {
    "et-excel": {
      "command": "pwsh",
      "args": [
        "-NoProfile",
        "-Command",
        "dotnet ${workspaceFolder}/Bin/ET.ExcelMcp.dll"
      ],
      "cwd": "${workspaceFolder}"
    }
  }
}
```

3. 保存后重启 Claude Desktop，`et-excel` 就会作为自定义工具列在侧边栏，可直接调用任意 Excel 能力。

### 2. Claude Code / VSCode

VSCode 插件读取仓库内的 `.claude/settings.local.json`。保留已有的 `permissions`，并添加：

```jsonc
{
  "permissions": { /* ... */ },
  "mcpServers": {
    "et-excel": {
      "command": "pwsh",
      "args": [
        "-NoProfile",
        "-Command",
        "dotnet ${workspaceFolder}/Bin/ET.ExcelMcp.dll"
      ],
      "cwd": "${workspaceFolder}"
    }
  }
}
```

这样即可在 Claude Code 面板中直接点选 `excel_*` 工具。

### 3. 命令行快速验证

MCP 服务采用 STDIO 协议，可以通过 PowerShell 将 JSON 请求直接管道到进程：

```powershell
$serverDll = Join-Path $PWD "Bin/ET.ExcelMcp.dll"
$payload = @'
{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","clientInfo":{"name":"cli","version":"1.0.0"}}}
{"jsonrpc":"2.0","id":2,"method":"tools/list"}
'@

$payload | dotnet $serverDll
```

输出中可看到 `tools/list` 的完整工具清单，用于确认安装是否成功。

## 可用工具

### 1. excel_file_operations - 文件操作

**支持的操作:**
- `create`: 创建新的Excel工作簿
- `convert`: 转换Excel文件格式 (支持CSV)

**示例:**
```json
// 创建工作簿
{
  "operation": "create",
  "path": "new.xlsx",
  "sheetName": "Sheet1"  // 可选
}

// 转换为CSV
{
  "operation": "convert",
  "inputPath": "book.xlsx",
  "outputPath": "book.csv",
  "format": "csv"
}
```

### 2. excel_sheet - 工作表操作

**支持的操作:**
- `create`: 创建新工作表
- `delete`: 删除工作表
- `rename`: 重命名工作表
- `list`: 列出所有工作表及其信息

**示例:**
```json
// 创建工作表
{
  "operation": "create",
  "path": "book.xlsx",
  "sheetName": "NewSheet"
}

// 删除工作表
{
  "operation": "delete",
  "path": "book.xlsx",
  "sheetIndex": 1
}

// 重命名工作表
{
  "operation": "rename",
  "path": "book.xlsx",
  "sheetIndex": 0,
  "newName": "Renamed"
}

// 列出工作表
{
  "operation": "list",
  "path": "book.xlsx"
}
```

### 3. excel_cell - 单元格操作

**支持的操作:**
- `write`: 写入单元格值
- `edit`: 编辑单元格值或公式
- `get`: 读取单元格值
- `clear`: 清空单元格

**示例:**
```json
// 写入单元格
{
  "operation": "write",
  "path": "book.xlsx",
  "sheetIndex": 0,  // 可选，默认为0
  "cell": "A1",
  "value": "Hello"
}

// 设置公式
{
  "operation": "edit",
  "path": "book.xlsx",
  "cell": "A3",
  "formula": "=A1+A2"
}

// 读取单元格
{
  "operation": "get",
  "path": "book.xlsx",
  "cell": "A1"
}

// 清空单元格
{
  "operation": "clear",
  "path": "book.xlsx",
  "cell": "A1"
}
```

### 4. excel_chart - 图表操作 ⭐ NEW

**支持的操作:**
- `add`: 创建新的图表，支持多序列以及分类轴范围
- `edit`: 修改图表类型、数据、标题、图例等
- `delete`: 删除指定索引的图表
- `get`: 获取当前工作表中所有图表的详细信息
- `update_data`: 更新图表的数据区域和分类轴
- `set_properties`: 设置标题、图例可见性/位置等属性

**示例:**
```json
// 添加柱状图
{
  "operation": "add",
  "path": "book.xlsx",
  "chartType": "Column",
  "dataRange": "B1:C10",
  "categoryAxisDataRange": "A1:A10",
  "title": "销售趋势"
}

// 更新图表属性
{
  "operation": "set_properties",
  "path": "book.xlsx",
  "chartIndex": 0,
  "title": "收入对比",
  "legendVisible": true,
  "legendPosition": "Right"
}
```

### 5. excel_comment - 批注操作 ⭐ NEW

**支持的操作:**
- `add`: 添加批注
- `edit`: 编辑批注
- `delete`: 删除批注
- `get`: 获取批注信息

**示例:**
```json
// 添加批注
{
  "operation": "add",
  "path": "book.xlsx",
  "cell": "A1",
  "comment": "注意这个值"
}

// 获取所有批注
{
  "operation": "get",
  "path": "book.xlsx"
}
```

### 6. excel_hyperlink - 超链接操作 ⭐ NEW

**支持的操作:**
- `add`: 添加单元格超链接
- `edit`: 修改超链接 (按单元格或索引)
- `delete`: 删除超链接
- `get`: 获取当前工作表超链接列表

**示例:**
```json
{
  "operation": "add",
  "path": "book.xlsx",
  "cell": "B2",
  "url": "https://example.com",
  "displayText": "官网"
}
```

### 7. excel_image - 图片操作 ⭐ NEW

**支持的操作:**
- `add`: 按指定单元格插入图片，可控制宽高/是否保持比例
- `delete`: 根据索引删除图片
- `get`: 返回图片元数据 (位置、尺寸、名称)
- `extract`: 将图片导出为独立文件

**示例:**
```json
{
  "operation": "add",
  "path": "book.xlsx",
  "imagePath": "logo.png",
  "cell": "C3",
  "width": 200,
  "keepAspectRatio": true
}
```

### 8. excel_freeze_panes - 冻结窗格 ⭐ NEW

**支持的操作:**
- `freeze`: 冻结前 N 行/N 列
- `unfreeze`: 取消冻结
- `get`: 获取当前冻结状态

**示例:**
```json
{
  "operation": "freeze",
  "path": "book.xlsx",
  "row": 1,
  "column": 2
}
```

### 9. excel_group - 行列分组 ⭐ NEW

**支持的操作:**
- `group_rows`, `ungroup_rows`: 分组/取消分组指定行区间
- `group_columns`, `ungroup_columns`: 分组/取消分组列区间
- `isCollapsed`: 控制是否初始折叠

**示例:**
```json
{
  "operation": "group_rows",
  "path": "book.xlsx",
  "startRow": 2,
  "endRow": 5,
  "isCollapsed": true
}
```

### 10. excel_pivot_table - 数据透视表 ⭐ NEW

**支持的操作:**
- `create`: 基于数据范围创建数据透视表，可配置行/列/筛选/值字段
- `configure`: 修改现有数据透视表的字段布局
- `refresh`: 刷新缓存并重新计算
- `get`: 获取当前工作表中所有数据透视表信息

**示例:**
```json
{
  "operation": "create",
  "path": "sales.xlsx",
  "dataRange": "A1:D20",
  "rowFields": ["地区"],
  "columnFields": ["产品"],
  "dataFields": [
    { "field": "销售额", "function": "sum", "name": "销售额合计", "format": "#,##0" }
  ]
}

// 更新字段
{
  "operation": "configure",
  "path": "sales.xlsx",
  "pivotIndex": 0,
  "pageFields": ["年份"],
  "dataFields": [
    { "field": "数量", "function": "count", "name": "订单数量" }
  ]
}
```

### 11. excel_conditional_formatting - 条件格式 ⭐ NEW

**支持的操作:**
- `add`: 添加多种条件格式 (比较、文本、色阶、数据条、图标集、自定义公式)
- `clear`: 清除指定范围内的所有条件格式规则
- `delete`: 按索引删除单个条件格式规则
- `get`: 获取所有条件格式的概览信息

**示例:**
```json
// 色阶
{
  "operation": "add",
  "path": "book.xlsx",
  "range": "B2:B20",
  "ruleType": "color_scale",
  "scaleType": "three_color",
  "lowColor": "#63BE7B",
  "midColor": "#FFEB84",
  "highColor": "#F8696B"
}

// 删除索引 0 的规则
{
  "operation": "delete",
  "path": "book.xlsx",
  "ruleIndex": 0
}
```

### 12. excel_protect - 保护与锁定 ⭐ NEW

**支持的操作:**
- `protect_sheet`: 对指定工作表启用保护，可配置是否允许排序、筛选等权限
- `unprotect_sheet`: 取消工作表保护
- `protect_workbook`: 锁定工作簿结构、窗口或修订
- `unprotect_workbook`: 取消工作簿级别的保护
- `lock_cells`: 将一个或多个单元格/区域设置为“锁定”
- `unlock_cells`: 将单元格/区域设置为“未锁定”

**示例:**
```json
// 保护工作表并允许排序/筛选
{
  "operation": "protect_sheet",
  "path": "book.xlsx",
  "sheetIndex": 0,
  "allowSorting": true,
  "allowFiltering": true
}

// 锁定工作簿结构与窗口
{
  "operation": "protect_workbook",
  "path": "book.xlsx",
  "lockStructure": true,
  "lockWindows": true
}

// 解锁多个区域
{
  "operation": "unlock_cells",
  "path": "book.xlsx",
  "ranges": ["A1:B2", "D5"]
}
```

### 13. excel_properties - 文档属性 ⭐ NEW

**支持的操作:**
- `get_workbook_properties`: 获取标题、作者、关键字、自定义属性等文档元数据
- `set_workbook_properties`: 设置上述元数据并支持写入自定义属性
- `get_sheet_properties`: 查看单个工作表的可见性、标签颜色、打印区域、图表/图片数量
- `edit_sheet_properties`: 重命名、隐藏/显示、设置标签颜色并选中某个工作表
- `get_sheet_info`: 获取所有工作表的索引、使用范围、分页设置等概览

**示例:**
```json
// 更新工作簿属性
{
  "operation": "set_workbook_properties",
  "path": "book.xlsx",
  "outputPath": "book_props.xlsx",
  "title": "月度报告",
  "author": "Alice",
  "customProperties": {
    "Department": "Finance",
    "Reviewed": true
  }
}

// 获取工作表概览
{
  "operation": "get_sheet_info",
  "path": "book_props.xlsx"
}
```

### 14. excel_view_settings - 视图设置 ⭐ NEW

**支持的操作:**
- `set`: 修改缩放、网格线、标题、视图模式等
- `get`: 查询指定工作表当前的视图设置快照

**示例:**
```json
// 设置视图
{
  "operation": "set",
  "path": "book.xlsx",
  "sheetIndex": 0,
  "zoom": 140,
  "showGridLines": false,
  "showHeadings": false,
  "viewType": "pagelayout"
}

// 获取视图
{
  "operation": "get",
  "path": "book.xlsx",
  "sheetIndex": 0
}
```

### 15. excel_print_settings - 打印设置 ⭐ NEW

**支持的操作:**
- `page_setup`: 设置页面方向、纸张大小、缩放、页边距
- `header_footer`: 配置页眉页脚 (左/中/右段文本)
- `print_area`: 设置或清除打印区域
- `get`: 获取打印设置、页眉页脚及打印区域信息

**示例:**
```json
// 页面设置
{
  "operation": "page_setup",
  "path": "book.xlsx",
  "orientation": "landscape",
  "paperSize": "A4",
  "fitToPagesWide": 1,
  "fitToPagesTall": 2
}

// 页眉页脚
{
  "operation": "header_footer",
  "path": "book.xlsx",
  "header": { "center": "销售报告" },
  "footer": { "left": "&[Page]" }
}

// 打印区域
{
  "operation": "print_area",
  "path": "book.xlsx",
  "range": "A1:D50"
}
```

### 16. excel_get_cell_address - 地址转换 ⭐ NEW

**支持的操作:**
- `cellAddress`: 输入 A1 地址 (如 `B2`) 返回对应的零基行列索引
- `row` + `column`: 输入零基行列索引 (如 `row=0, column=0`) 返回 A1 地址

**示例:**
```json
// 地址转索引
{
  "tool": "excel_get_cell_address",
  "arguments": { "cellAddress": "AA10" }
}

// 索引转地址
{
  "tool": "excel_get_cell_address",
  "arguments": { "row": 5, "column": 2 }
}
```

## MCP 协议使用示例

### 初始化服务器

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "initialize",
  "params": {
    "protocolVersion": "2025-11-25",
    "clientInfo": {
      "name": "your-client",
      "version": "1.0.0"
    }
  }
}
```

### 列出所有工具

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/list"
}
```

### 调用工具

```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "tools/call",
  "params": {
    "name": "excel_cell",
    "arguments": {
      "operation": "write",
      "path": "/path/to/file.xlsx",
      "cell": "A1",
      "value": "Hello World"
    }
  }
}
```

## 架构说明

### 目录结构

```
ET.ExcelMcp/
├── Core/
│   ├── ArgumentHelper.cs      # 参数解析、类型推断
│   ├── ExcelHelper.cs         # 通用 EPPlus 操作封装
│   ├── IExcelTool.cs          # 工具接口/注解约定
│   ├── McpErrorHandler.cs     # JSON-RPC 错误转换
│   ├── McpModels.cs           # MCP 数据模型
│   ├── McpServer.cs           # STDIO 服务器实现
│   ├── SecurityHelper.cs      # 路径/输入安全校验
│   ├── ToolRegistry.cs        # 自动发现工具
│   └── VersionHelper.cs       # 版本号读取
├── Tools/
│   ├── ExcelCellTool.cs
│   ├── ...
│   └── ExcelViewSettingsTool.cs
├── Tests/
│   ├── Helpers/               # MSTest 基类与断言适配
│   ├── Excel/                 # 覆盖 25 个工具的单测
│   └── ET.ExcelMcp.Tests.csproj
├── Program.cs                 # 入口程序
├── ET.ExcelMcp.csproj
```

### 工具自动发现

本服务器使用反射机制自动发现和注册所有实现了 `IExcelTool` 接口的工具类：

1. 工具类名会自动转换为snake_case格式（如：`ExcelCellTool` → `excel_cell`）
2. 工具类必须位于 `ET.ExcelMcp.Tools` 命名空间下
3. 工具类必须实现 `IExcelTool` 接口

### 类型自动转换

`ArgumentHelper.ParseValue` 方法会自动识别并转换以下类型：
- **数字**: 使用 `double.TryParse` 识别
- **布尔值**: 使用 `bool.TryParse` 识别
- **日期**: 使用 `DateTime.TryParse` 识别
- **字符串**: 默认类型

## 许可证

本项目使用 EPPlus 的非商业许可证。如果需要商业使用，请修改 `Program.cs` 中的许可证设置：

```csharp
ExcelPackage.LicenseContext = LicenseContext.Commercial;
```

## 参考

- [EPPlus 官方文档](https://github.com/EPPlusSoftware/EPPlus)
- [MCP 协议规范](https://spec.modelcontextprotocol.io/)
- 参考实现: aspose-mcp-server

## 与 aspose-mcp-server 的差异

- **依赖替换**：核心由 Aspose.Cells 换成 EPPlus 7.5.2，API 层保持一致。遇到 Aspose 才支持的 API 时，可在 `ExcelHelper` 中补充等价封装后再调用。
- **项目位置**：此实现位于 `Packages/cn.etetet.config/DotNet~/ET.ExcelMcp` 并参与 `ET.sln`，无需单独仓库；所有输出统一写入 `Bin`。
- **安全策略**：新增 `SecurityHelper`、`ArgumentHelper` 等通用校验逻辑，防止路径穿越、过大数组/字符串导致的资源耗尽。
- **测试体系**：使用 `Tests/ET.ExcelMcp.Tests.csproj` 覆盖 25 个工具，与原 aspose 仓库的脚本式测试不同。

## 实现进度

### ✅ 已实现 (25/25 工具)

当前仓库的 25 个 `excel_*` 工具均已落地

- [x] **excel_cell** - 单元格操作
- [x] **excel_sheet** - 工作表操作
- [x] **excel_file_operations** - 文件操作

- [x] **excel_range** - 范围操作
  - 批量写入/读取数据 (write, edit, get)
  - 清空、复制、移动范围
  - 复制格式到其他范围

- [x] **excel_style** - 样式操作
  - 设置字体 (名称、大小、颜色、加粗、斜体)
  - 单元格背景色、边框
  - 对齐方式 (水平、垂直)
  - 获取和复制样式

- [x] **excel_row_column** - 行列操作
  - 插入/删除行列
  - 隐藏/显示行列
  - 设置行高/列宽
  - 自动调整大小

- [x] **excel_merge_cells** - 合并单元格
  - 合并/取消合并单元格
  - 获取合并区域信息

- [x] **excel_formula** - 公式操作 (增强版)
  - 添加公式
  - 获取公式和计算结果
  - 批量计算所有公式
  - 数组公式支持

- [x] **excel_data_operations** - 数据操作
  - 排序 (升序/降序)
  - 查找/替换
  - 去重

- [x] **excel_filter** - 筛选功能
  - 自动筛选
  - 高级筛选
  - 清除筛选

- [x] **excel_data_validation** - 数据验证
  - 下拉列表
  - 数值范围验证
  - 自定义验证规则

- [x] **excel_named_range** - 命名范围
  - 创建/删除命名范围
  - 获取命名范围列表
  - 引用命名范围

- [x] **excel_chart** - 图表操作
  - 创建图表 (柱状图、折线图、饼图等)
  - 设置图表数据源
  - 修改图表样式

- [x] **excel_pivot_table** - 数据透视表
  - 创建/配置数据透视表 (行/列/筛选/值字段)
  - 自定义聚合函数与命名
  - 刷新缓存与计算，导出信息

- [x] **excel_conditional_formatting** - 条件格式
  - 颜色比较、文本匹配、色阶、数据条、图标集
  - 自定义公式及样式 (字体/背景/加粗/斜体)
  - 查询、清除、按索引删除规则

- [x] **excel_comment** - 批注操作
  - 添加/删除批注
  - 编辑批注内容
  - 显示/隐藏批注

- [x] **excel_hyperlink** - 超链接
  - 添加/删除超链接
  - 修改超链接目标
  - 获取超链接列表

- [x] **excel_image** - 图片操作
  - 插入图片
  - 调整图片大小和位置
  - 删除/导出图片

- [x] **excel_freeze_panes** - 冻结窗格
  - 冻结行/列
  - 取消冻结
  - 获取状态

- [x] **excel_group** - 分组功能
  - 行/列分组
  - 展开/折叠分组
  - 删除分组

- [x] **excel_protect** - 保护功能
  - 工作表保护/取消保护
  - 工作簿保护
  - 单元格锁定/解锁

- [x] **excel_properties** - 文档属性
  - 设置/获取文档元数据
  - 作者、标题、主题、关键字

- [x] **excel_view_settings** - 视图设置
  - 缩放级别
  - 显示/隐藏网格线
  - 切换普通/分页预览/页面布局视图

- [x] **excel_print_settings** - 打印设置
  - 页面设置 (纸张大小、方向)
  - 页眉页脚
  - 打印区域

- [x] **excel_get_cell_address** - 地址转换
  - 行列号转单元格地址 (0,0 → A1)
  - 单元格地址转行列号 (A1 → 0,0)

## 测试

本项目提供 `Tests/ET.ExcelMcp.Tests.csproj`（MSTest）覆盖 25 个工具的核心读写路径与安全校验，可以直接在仓库根目录执行：

```powershell
dotnet test Packages/cn.etetet.config/DotNet~/ET.ExcelMcp/Tests/ET.ExcelMcp.Tests.csproj
```

测试会自动生成临时 Excel 文件并验证以下内容：
- ✅ MCP 初始化/`tools/list`
- ✅ 各 `excel_*` 工具的读写、过滤、格式、保护等主流程
- ✅ 错误处理与输入校验（如非法路径、越界索引）
- ✅ 透视表/条件格式/图片/批注等复杂对象的序列化

## 开发说明

### 添加新工具

1. 在 `Tools/Excel/` 目录下创建新的工具类
2. 实现 `IExcelTool` 接口
3. 定义 `Description` 和 `InputSchema` 属性
4. 实现 `ExecuteAsync` 方法
5. 工具会自动被 `ToolRegistry` 发现和注册

### 错误处理

所有工具方法应该抛出适当的异常：
- `ArgumentException`: 参数错误
- `ArgumentNullException`: 必需参数缺失
- `FileNotFoundException`: 文件不存在
- `InvalidOperationException`: 操作无效

这些异常会被 `McpErrorHandler` 自动转换为标准的JSON-RPC错误响应。
