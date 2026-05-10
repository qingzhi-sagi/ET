# cn.etetet.harness

## 概述

AI Harness 技能分发包，包含 ET 项目使用的技能路由索引、轻量入口和详细规则引用。

## 使用方式

- 技能索引位于 `skills/index.md`。
- 新项目接入时，请在项目根级 `AGENTS.md` 中要求先读取 `Packages/cn.etetet.harness/skills/index.md`。
- 如果项目仍保留根目录 `skills`，请以包内内容为分发源，按需同步到根目录。

## 维护规则

- 修改技能入口或引用文档前，先读取 `skills/index.md`。
- 新增技能时，同步更新 `skills/index.md` 的路由场景。
- 本包不承载运行时代码，不应新增业务依赖。
- 不手工生成 `.meta` 或修改 `.csproj`，需要时通过 Unity 刷新生成。
