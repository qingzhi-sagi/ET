# com.etetet.init

## 概述

初始化包，包含 Unity 初始化配置、CodeMode 切换入口，以及生成各包 `AssemblyReference.asmref` 的命令行工具。

## 目录约定

- `Runtime`：初始化运行时代码与 `GlobalConfig`。
- `Editor`：初始化包自己的 Unity 编辑器入口，保持独立程序集，不汇入 `ET.Editor`。
- `DotNet~`：`ET.CodeMode` 命令行工具。

## 开发约定

- 修改 `DotNet~/CodeModeChangeHelper.cs` 后，必须确认 `Scripts/Editor/Share` 会生成 `{ "reference": "ET.Editor" }`。
- 本包的 `Editor` 目录不参与 `ET.Editor` 汇聚迁移。
