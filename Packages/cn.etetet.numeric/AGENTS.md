# cn.etetet.numeric

## 概述

数值系统包，提供 `NumericComponent`、数值变化分发、数值监听器注册，以及 `NumericType` 的 Luban 数据定义与导出。

## 本包规则

- 主要规范以 `Packages/cn.etetet.harness/AGENTS.md` 为准。
- 修改 `Luban/Config/**`、`DotNet~/ExcelExporter/**` 或导出产物后，按 `et-luban` 流程重新执行导出。
- 修改 `Scripts/Model/**` 或 `Scripts/Hotfix/**` 后，按 `et-code` 与 `et-build` 流程验证。
- `NumericType.cs` 是生成文件，不手工维护常量内容；数值类型来源为 `Luban/Config/Datas/NumericType.xlsx`，由 `DotNet~/ExcelExporter/ExcelHandler_NumericType.cs` 转成 Luban XML 后导出。
