# cn.etetet.spell

## 概述

技能与 Buff 系统包，包含 Spell/Buff 配置模型、运行时施法逻辑、客户端表现组件，以及 spell 相关 Unity 编辑器工具。

## 开发规则

- 遵守根目录 `AGENTS.md` 与 `Packages/cn.etetet.harness/AGENTS.md`。
- 所有命令必须使用 `pwsh`。
- C# 文件默认一类一文件。
- 新增编辑器代码放在 `Scripts/Editor/Share/` 下，编译进 `ET.Editor`。
- 不手工创建 `.meta`，由 Unity 刷新生成。
- 不手工修改 `.csproj`。
- 修改 Spell/Buff 配置源时，当前资产来源为 `Packages/cn.etetet.statesync/Assets/BT/**`。
- 导出 Spell/Buff 配置时复用现有 ScriptableObject 导出链路。
