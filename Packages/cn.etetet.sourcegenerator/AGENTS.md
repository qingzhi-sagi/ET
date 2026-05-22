# cn.etetet.sourcegenerator

## 概述

ET SourceGenerator 包，承载 Roslyn Source Generator、Analyzer、CodeFixer 与生成器标记类型。

## 开发约定

- 通用代码规则以 `Packages/cn.etetet.harness/AGENTS.md` 和 `et-code` skill 为准。
- 分析器和生成器修改后必须使用 `dotnet build ET.sln` 验证。
- 新增诊断时同步维护 `Config/DiagnosticIds.cs` 与 `Config/DiagnosticRules.cs`。
- 不手工生成 `.meta` 或修改 Unity 工程文件。
