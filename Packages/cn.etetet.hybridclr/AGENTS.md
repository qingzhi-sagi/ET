# cn.etetet.hybridclr

## 概述

HybridCLR 集成包，包含热更新相关运行时、插件和编辑器工具。

## 详细文档

- **代码规范**：请查看 `/et-code` skill。
- **构建验证**：请查看 `/et-build` skill。

## 目录约定

- `Scripts/Editor`：编辑器代码，通过 `AssemblyReference.asmref` 汇入 `ET.Editor`。
- 不再使用包根 `Editor` 目录承载编辑器 C# 代码。
