# cn.etetet.recast

## 概述

Recast 寻路相关包，包含运行时代码与编辑器辅助工具。

## 目录约定

- `Scripts/Model`：Recast 模型代码。
- `Scripts/Hotfix`：Recast 热更代码。
- `Scripts/Editor`：编辑器代码，通过 `.asmref` 汇入 `ET.Editor`。

## 开发约定

- 遵循 `cn.etetet.harness` 的 `/et-code` 规则。
- 新增编辑器代码不得新建独立编辑器 asmdef。
