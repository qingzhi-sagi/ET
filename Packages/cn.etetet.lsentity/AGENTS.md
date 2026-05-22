# cn.etetet.lsentity

## 概述

锁步 Entity 包，承载 `LSEntity`、`LSWorld` 及锁步 Entity 基础能力。

## 开发约定

- 通用 Entity / System / Analyzer 规则以 `Packages/cn.etetet.harness/AGENTS.md` 和 `et-code` skill 为准。
- 修改 AddComponent / AddChild 包装方法时，必须与 `cn.etetet.core` 的 `Entity` API 保持命名和约束一致。
- 不手工生成 `.meta` 或修改 `.csproj`。
