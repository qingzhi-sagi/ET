# ET Skills 索引

先读本文件，禁止先完整读取所有 skill 正文。

## 加载策略

1. 先用场景匹配到 1 个主 skill；只有跨域任务才叠加其它 skill。
2. 先读命中的轻量入口；需要细节时再读 skill 正文的完整规范部分。
3. 能直接调用现成脚本时，优先脚本，不重复展开长命令。
4. 测试相关任务统一入口：使用场景较重时优先 `et-tdd`，只执行测试用 `et-test-run`，只写测试用 `et-test-write`。
5. 涉及 Unity 编辑器内操作时，优先使用 `et-unitybridge`。

## 任务入口

### 核心开发

- `et-code`
  - 场景：Entity / Component / System / Helper、包依赖、程序集、Handler 结构、组件契约、Module analyzer、分析器报错、新建或移动 C# 文件
- `et-async`
  - 场景：`async` / `await` / `ETTask` / `ETCancellationToken`、`EntityRef` await 安全、`NewContext(...)`、并发等待
  - 注意：改任何含 `async`/`ETTask` 的代码时，叠加此 skill

### Unity 编辑器操作

- `et-unitybridge`
  - 场景：查询 Unity 心跳/状态、触发编译/刷新/重新生成项目/进入退出 PlayMode/热更新 Reload
  - 在执行需要 Unity 配合的操作前，先用此 skill 确认编辑器已就绪

### 构建与导出

- `et-build`
  - 场景：编译（`dotnet build ET.sln`）、刷新 `.csproj`、导出 Excel/Proto 配置、启动服务器、发布

### 提交整理

- `et-git`
  - 场景：提交前检查、整理 `git status` / `git diff`、筛除无关文件、编写中文提交信息

### 测试验证

- `et-tdd`
  - 场景：TDD 驱动开发、完整测试闭环（设计→编写→编译→运行→回归）
- `et-test-run`
  - 场景：只执行测试、查看测试日志、调试测试失败
- `et-test-write`
  - 场景：编写或修改测试用例

### 数据工具

- `et-excel`
  - 场景：通过 `ET.ExcelMcp` 读写 Excel、批量导入导出、样式 / 公式 / 图表 / 工作表操作

## 组合场景

- 改普通 ET 代码：`et-code` -> 涉及异步时叠加 `et-async` -> 必要时 `et-build` / `et-tdd`
- 只做异步链路梳理或 review：`et-async` -> 必要时 `et-code`
- 架构合规检查：`et-code`
- 创建新功能（完整闭环）：`et-code` -> 涉及异步叠加 `et-async` -> `et-build` -> `et-tdd`
- 做 Unity 编辑器内操作：`et-unitybridge` -> 确认就绪后执行对应操作
- 写通用测试：`et-tdd` 或 `et-test-write` + `et-test-run`
- 准备提交或审查 diff：`et-git`
- 完成开发准备提交：对应开发 skill -> 必要时 `et-build` / `et-test-run` -> `et-git`
- 只做导出或运行：`et-build`
- 只处理 Excel：`et-excel`
