# ET Code 参考

## 快速分流

- 文件、`.meta`、包落点：看“文件与包落点”
- ECS 数据 / 逻辑边界：看“ECS 分离”
- 组件存在性契约、低价值判空、Module analyzer：看“结构与契约”
- Handler `Run` 入口规范：看“Handler Run”
- 包依赖与层级：看“包依赖规则”和“包层级速查”
- 消息字段与 Handler 命名：看“消息与命名”

## 文件与包落点

- 包命名：`cn.etetet.{模块名}`；所有代码必须在 `Packages/cn.etetet.*` 下。
- 每个包必须有：`packagegit.json`（Id 全局唯一）、`Scripts/Model/Share/PackageType.cs`（编号与 Id 一致）、包内 `AGENTS.md`。
- 修改包内文件前先读取对应包的 `AGENTS.md`。
- Model 层：`Scripts/Model/` 或 `Scripts/ModelView/`。
- Hotfix 层：`Scripts/Hotfix/` 或 `Scripts/HotfixView/`。
- 新建 C# 文件不手工创建 `.meta`，由 Unity 自动生成。
- 移动 C# 文件必须同时移动对应 `.meta`，确保 GUID 不变。
- 默认一类一文件；主类型与文件名保持一致。
- 严禁手工修改 `.csproj`；需要刷新工程文件时通过 UnityBridge 或项目既有 Unity 流程。

## ECS 分离

- `Entity` / `Component` 只包含数据，禁止业务方法。
- `System`（静态 partial 类）包含逻辑，通过静态扩展方法实现。
- 复杂业务逻辑放 `XXXHelper`，不要无边界扩散进 System。
- Singleton 类（如 `RobotCaseDispatcher`）可以包含方法，不需要额外创建 System。
- 跨实体、跨模块、跨流程的编排优先抽到 `XXXHelper`。

## 静态状态

- 禁止为了省事新增 `static Dictionary`、`static List`、`static HashSet`、`static` 缓存字段或全局可变状态。
- Attribute 扫描结果、节点注册表、配置缓存、运行时缓存等必须有明确 owner。
- owner 优先使用 `Singleton<T>`、框架已有 Dispatcher、配置 Category、Scene/Fiber 组件或专门管理组件。
- 静态只读常量必须是不可变数据，不能承载运行时状态或热重载状态。
- 热重载、测试隔离、多 Fiber、多 Scene 场景下，进程级静态状态默认视为风险点。

## 结构与契约

- 先决定类应落在 `Model` / `ModelView` / `Hotfix` / `HotfixView` 哪一层。
- 组件来源必须清楚：由 `FiberInit_*` 预创建、当前方法 `AddComponent<T>()` 创建，或业务上本来可选。
- 组件按设计必须存在时直接使用；缺失应暴露 bug，不要偷偷补齐。
- 当前方法负责初始化组件时，直接 `AddComponent<T>()`，不要先判空。
- 只有业务上确实可选的组件才允许判空分支，并在代码里表达为什么可选。
- 新增 `if (x == null)`、`ThrowIfNull`、`?? throw`、`GetComponent<T>() ?? AddComponent<T>()` 前，先确认是否真有合法为空路径。
- 框架强契约对象不要重复做低价值判空；开放边界参数才适合 `ArgumentNullException.ThrowIfNull`。
- `TimerComponent`、`CoroutineLockComponent` 这类在 `Awake` 已回写 `Scene` 的组件，`AddComponent` 后不要再手工回填引用。

## Handler Run

- `MessageLocationHandler` / `MessageHandler<T>` / `MessageSessionHandler` 的 `Run` 入口无需额外判空实体。
- 如有 `await`，必须在 `await` 后通过 `EntityRef` 重新获取再使用。
- Handler 中的实体安全规则与普通异步代码一致。

## 包依赖规则

- 包之间只能单向依赖，不能循环；A 依赖 B，B 永远不能反向访问 A。
- 依赖变更时递归补齐，不要只加一层。
- 跨包访问必须显式声明依赖，不存在同层访问例外。
- 只能高层包依赖低层包。
- 遇到模块双向耦合时，优先拆公共 Helper、公共消息，或把调用入口上移到更高层。

## Module analyzer

- 类可通过 `Module(ModuleName.X)` 声明模块；未标注时默认属于 `Global`。
- A 模块调用 B 模块的方法后，B 模块不能反向调用 A；A 也不能直接读 B 的字段。
- 分析器报错先看模块边界、程序集落点、包依赖方向，再动代码。

## 包层级速查

- 第 5 层：`cn.etetet.wow`、`cn.etetet.btnode`、`cn.etetet.test`、`cn.etetet.robot`（依赖 console）、`cn.etetet.login`、`cn.etetet.mapplay`（依赖 map/spell/item/quest）。
- 第 4 层：`cn.etetet.actorlocation`（依赖 netinner）、`cn.etetet.aoi`（依赖 unit/numeric）、`cn.etetet.map`（依赖 aoi/unit/netinner/move/numeric）。
- 第 3 层：`cn.etetet.numeric`、`cn.etetet.move`、`cn.etetet.nav2d`（均依赖 unit）、`cn.etetet.netinner`（依赖 startconfig）、`cn.etetet.router`（依赖 startconfig/http）、`cn.etetet.item`（依赖 unit）。
- 第 2 层：`cn.etetet.unit`、`cn.etetet.behaviortree`、`cn.etetet.http`、`cn.etetet.startconfig`、`cn.etetet.console`、`cn.etetet.yooassets`、`cn.etetet.yiui*` 系列。
- 第 1 层：`cn.etetet.core`、`cn.etetet.excel`、`cn.etetet.proto`、`cn.etetet.loader`。

## 消息与命名

- 消息类字段统一使用 `PascalCase`。
- 命名空间按 package 组织，避免重复定义同名类。
- Handler 与消息类型命名要与 package 和职责保持一致。

## 常见错误

- 代码文件放到了 `Packages/cn.etetet.*` 之外。
- 移动 C# 文件时漏掉对应 `.meta`。
- 包缺少 `packagegit.json`、`PackageType.cs` 或包内 `AGENTS.md`。
- 在 `Entity` 中塞业务方法或流程状态机。
- `await` 后直接访问旧 `Entity`。
- 字段或集合中直接保存 `Entity`。
- 新类落到错误程序集层。
- AI 为注册表、缓存或映射关系创建静态可变字段。
- 把多个类写进同一个 `.cs`。
- 未检查包依赖方向与 Module analyzer 约束。
- 复制出重复类或重复消息定义。
- 用 hard code 绕过现有配置、节点或流程。
