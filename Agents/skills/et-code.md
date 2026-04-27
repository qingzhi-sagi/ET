# et-code - ET Code 入口

## 何时使用

- 新建或修改 `Entity`、`Component`、`System`、`Helper`
- 新建或修改 `MessageLocationHandler`、`MessageHandler<T>`、`MessageSessionHandler`
- 新建或移动 C# 文件、检查 `.meta` 与包内落点是否正确
- 新增消息、模块、程序集或包依赖
- 处理 ECS 分层、包依赖、程序集落点、分析器报错
- 评估代码是否违反 ECS 分层与包依赖规则

## 不要加载

- 只是执行测试、导出配置或操作 Excel
- 只做 Unity 编辑器操作（用 `et-unitybridge`）

## 快速分流

- 确认文件落点与 `.meta` 规则 → 读"关键规范 / 文件与包落点"
- 确认 ECS 数据/逻辑边界 → 读"关键规范 / ECS 分离"
- 涉及组件存在性契约、反兜底、Module analyzer → 读"关键规范 / 结构与契约"
- 涉及 `async` / `await` / `ETTask` → 叠加 `Agents/skills/et-async.md`
- 确认 Handler `Run` 规范 → 读"关键规范 / Handler Run"
- 确认包依赖层级 → 读"关键规范 / 包层级速查"
- 查消息字段命名或 Handler 命名 → 读"关键规范 / 消息与命名"

## 默认动作

1. 先确认改动所在 package 与程序集层级（`Packages/cn.etetet.*`）。
2. 新建 package、首次进入陌生 package、或分析器报错指向包元数据时，再检查 `packagegit.json`、`PackageType.cs`、包内 `AGENTS.md`。
3. 先检查是否违反 Entity / System 分离、Handler `Run` 规范、包依赖单向、禁止 hard code。
4. 涉及结构调整、组件契约或 Module analyzer 报错时，先判断职责边界、程序集落点与依赖方向，再动代码。
5. 涉及 `async` / `await` / `ETTask` / `ETCancellationToken` / `EntityRef` await 安全时，叠加 `Agents/skills/et-async.md`。
6. 新建类或节点前优先复用现有结构，不要顺手扩散职责。
7. **严禁 AI 手工生成 `.meta` 文件，也严禁手工修改 `.csproj`**；需要补齐 `.meta` 或刷新工程文件时，通过 `et-unitybridge` 的 `Refresh` 或 `RegenProject` 命令让 Unity 编辑器刷新生成。
8. **移动 C# 文件时必须同步移动对应 `.meta` 文件**，以保留 GUID；若 `.meta` 丢失，Unity 引用将断裂。
9. **每个类一个文件**；除同文件内天然绑定的极小枚举/结构外，禁止把多个类写进同一个 `.cs`。

## 关键规范

### 文件与包落点

- 包命名：`cn.etetet.{模块名}`；所有代码必须在 `Packages/cn.etetet.*` 下
- 每个包必须有：`packagegit.json`（Id 全局唯一）、`Scripts/Model/Share/PackageType.cs`（编号与 Id 一致）、`AGENTS.md`
- Model 层：`Scripts/Model/` 或 `Scripts/ModelView/`
- Hotfix 层：`Scripts/Hotfix/` 或 `Scripts/HotfixView/`
- 新建 C# 文件不需要手动创建 `.meta`，由 Unity 自动生成
- 移动 C# 文件时必须同时移动对应 `.meta`，确保 GUID 不变
- 默认一类一文件；主类型与文件名保持一致，避免一个文件里堆多个类

### ECS 分离

- `Entity` / `Component`：只包含数据，禁止业务方法
- `System`（静态 partial 类）：只包含逻辑，通过静态扩展方法实现
- 复杂业务逻辑放 `XXXHelper`，不要无边界扩散进 System
- Singleton 类（如 `RobotCaseDispatcher`）可以包含方法，不需要额外创建 System
- 跨实体、跨模块、跨流程的编排优先抽到 `XXXHelper`，不要继续膨胀某个 `System`

### 结构与契约

- 先决定类应落在 `Model` / `ModelView` / `Hotfix` / `HotfixView` 哪一层，再开始写代码
- 组件来源必须先想清楚：由 `FiberInit_*` 预创建、由当前方法 `AddComponent<T>()` 创建，或业务上本来就可选
- 组件按设计必须存在时直接使用；缺失应暴露 bug，不要偷偷补齐
- 当前方法负责初始化组件时，直接 `AddComponent<T>()`，不要先判空
- 只有业务上确实可选的组件才允许判空分支，并且要在代码里表达“为什么可选”
- 新增 `if (x == null)`、`ThrowIfNull`、`?? throw`、`GetComponent<T>() ?? AddComponent<T>()` 前，先确认当前调用链是否真的存在合法为空路径
- 框架强契约对象不要重复做低价值判空；开放边界参数才适合 `ArgumentNullException.ThrowIfNull`
- `TimerComponent`、`CoroutineLockComponent` 这类在 `Awake` 已回写 `Scene` 的组件，`AddComponent` 后不要再手工回填引用

### Handler Run 规范

- `MessageLocationHandler` / `MessageHandler<T>` / `MessageSessionHandler` 的 `Run` 入口无需额外判空实体
- 如有 `await`，必须在 `await` 后通过 `EntityRef` 重新获取再使用（无豁免，与普通异步代码规则一致）

### 包依赖规则

- 包之间只能单向依赖，不能循环；A 依赖 B，B 永远不能反向访问 A
- 依赖变更时递归补齐，不要只加一层
- 跨包访问必须显式声明依赖，不存在同层访问例外

### Module analyzer

- 类可通过 `Module(ModuleName.X)` 声明模块；未标注时默认属于 `Global`
- A 模块调用 B 模块的方法后，B 模块不能反向调用 A；A 也不能直接读 B 的字段
- 遇到模块双向耦合时，优先拆公共 `Helper`、公共消息，或把调用入口上移到更高层

### Entity 引用边界

- 字段、属性、集合里保存 `EntityRef<T>`，不要长期持有 `Entity`
- `await` 后重新获取实体、并发等待、`NewContext(...)` 等异步安全细节统一看 `et-async`

### 包层级速查

- 第 5 层：`cn.etetet.wow`、`cn.etetet.btnode`、`cn.etetet.test`、`cn.etetet.robot`（依赖 console）、`cn.etetet.login`、`cn.etetet.mapplay`（依赖 map/spell/item/quest）
- 第 4 层：`cn.etetet.actorlocation`（依赖 netinner）、`cn.etetet.aoi`（依赖 unit/numeric）、`cn.etetet.map`（地图基础系统）
- 第 3 层：`cn.etetet.numeric`、`cn.etetet.move`、`cn.etetet.nav2d`（均依赖 unit）、`cn.etetet.netinner`（依赖 startconfig）、`cn.etetet.router`（依赖 startconfig/http）、`cn.etetet.item`（依赖 unit）
- 第 2 层：`cn.etetet.unit`、`cn.etetet.behaviortree`、`cn.etetet.http`、`cn.etetet.startconfig`、`cn.etetet.console`、`cn.etetet.yooassets`、`cn.etetet.yiui*`（全系列）
- 第 1 层：`cn.etetet.core`、`cn.etetet.excel`、`cn.etetet.proto`、`cn.etetet.loader`

### 消息与命名

- 消息类字段统一使用 `PascalCase`
- 命名空间按 package 组织，避免重复定义同名类
- Handler 与消息类型命名要与 package 和职责保持一致

## 常见错误

- 代码文件放到了 `Packages/cn.etetet.*` 之外
- 移动 C# 文件时漏掉对应 `.meta`
- 包缺少 `packagegit.json`、`PackageType.cs` 或包内 `AGENTS.md`
- 在 `Entity` 中塞业务方法或流程状态机
- `await` 后直接访问旧 `Entity` 变量
- 字段或集合中直接保存 `Entity`（应改为 `EntityRef<T>`）
- 新类落到错误的程序集层
- 把多个类写进同一个 `.cs`，导致职责混杂、后续移动困难
- 未检查包依赖方向与 Module analyzer 约束
- 复制出重复类或重复消息定义
- 用 hard code 绕过现有配置、节点或流程

## 按需补读

- `Agents/skills/et-async.md`：异步边界、`EntityRef`、`ETTask`、`ETCancellationToken`
