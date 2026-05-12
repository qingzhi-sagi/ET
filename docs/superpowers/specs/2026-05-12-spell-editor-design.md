# Spell 表格式编辑器设计

## 背景

spell 包已有 `SpellConfig` 与 `BuffConfig`，运行时代码通过生成的 `CodeMode/Config/**/SpellConfigCategoryFactory_Config.cs` 和 `BuffConfigCategoryFactory_Config.cs` 加载配置。当前实际编辑源不是 spell 包内的 Luban xlsx 数据表，而是 `Packages/cn.etetet.statesync/Assets/BT/**` 下的 `SpellScriptableObject` 与 `BuffScriptableObject`。现有导出链路位于 `cn.etetet.behaviortree` 包的 `ExportScriptableObjectEditor.ExportScriptableObject()`，它扫描 ScriptableObject 并生成 spell 包的配置工厂代码。

本编辑器的目标是在 spell 包提供一个 Unity 编辑器窗口，以类似 Luban 表格的方式编辑技能链信息，同时继续沿用现有 ScriptableObject 数据源与导出链路。

## 目标

- 在 spell 包新增可编辑的 Unity EditorWindow。
- 继续管理 `Packages/cn.etetet.statesync/Assets/BT/**` 中的 Spell/Buff ScriptableObject，不迁移现有资产。
- 通过两个表展示当前主技能链路中的所有技能与 Buff。
- 支持完整增删改：新建、复制、删除、重命名、修改 Id，并维护目录、资产名与引用关系。
- 支持保存资产并一键导出到现有 `CodeMode/Config` 生成代码。

## 非目标

- 不修改 Luban Excel 表结构。
- 不把 Spell/Buff 资产迁移到 spell 包。
- 不手工创建 Unity `.meta` 文件。
- 不修改 `.csproj`。
- 不在表格单元格里直接编辑行为树节点结构；行为树继续通过现有 BehaviorTreeEditor 编辑。

## 数据源与落点

编辑器代码放在 `Packages/cn.etetet.spell/Scripts/Editor/...`，编译进现有 `ET.Editor`。spell 包当前缺少包内 `AGENTS.md`，实现阶段需要先补齐该文件。

数据资产继续扫描：

```text
Packages/cn.etetet.statesync/Assets/BT/**
```

编辑器建立两个索引：

- `SpellId -> SpellScriptableObject`
- `BuffId -> BuffScriptableObject`

保存时对被修改的 ScriptableObject 调用 `EditorUtility.SetDirty`，再调用 `AssetDatabase.SaveAssets`。导出时复用：

```csharp
ET.Client.ExportScriptableObjectEditor.ExportScriptableObject()
```

## 窗口结构

菜单入口建议为：

```text
ET/Spell/Spell Editor
```

窗口顶部工具栏包含：

- 刷新
- 主技能选择或搜索
- 新建主技能
- 新建子技能
- 新建 Buff
- 复制链
- 删除
- 保存
- 导出配置
- 只看异常

主体只保留两个表：

1. `SpellConfig 表`
2. `BuffConfig 表`

不再设计独立引用表。引用到的技能进入 `SpellConfig 表`，引用到的 Buff 进入 `BuffConfig 表`。

## SpellConfig 表

`SpellConfig 表`显示当前选中主技能链路内的所有技能，包括：

- 选中的主技能。
- 主技能、子技能及其递归链路中通过 `BTCreateSpell.SpellConfigId` 引用到的技能。
- 缺失引用行，例如 `Missing Spell 100999`。

字段列建议包含：

- `Id`
- 主/子类型，主技能规则为 `Id % 10 == 0`，子技能规则为 `Id % 10 != 0`
- `Desc`
- `IconName`
- `CD`
- `DamageMultiplier`
- `BuffId`
- `Cost` 摘要
- `TargetSelector` 摘要
- 来源说明
- 资产路径
- 校验状态

普通字段可直接表格编辑。`Cost` 与 `TargetSelector` 显示类型和节点数量摘要，双击打开现有 BehaviorTreeEditor。

## BuffConfig 表

`BuffConfig 表`显示当前技能链路内的所有相关 Buff，包括：

- 技能自身 `BuffId` 对应的 Buff。
- 行为树中通过 `BTAddBuff.ConfigId` 等 Buff 引用字段引用到的 Buff。
- 缺失引用行，例如 `Missing Buff 209999`。

字段列建议包含：

- `Id`
- `Desc`
- `Duration`
- `TickTime`
- `MaxStack`
- `Stack`
- `OverLayRuleType`
- `Flags`
- `NoticeType`
- `Effects` 摘要
- 来源说明
- 资产路径
- 校验状态

普通字段可直接表格编辑。枚举字段使用下拉，`Flags` 使用多选。`Effects` 显示效果节点数量与类型摘要，双击对应 Effect 打开现有 BehaviorTreeEditor。

## 链路构建

用户选择主技能后，编辑器从该主技能开始构建链路：

1. 加入主 `SpellConfig`。
2. 加入该技能 `BuffId` 对应的 `BuffConfig`。
3. 递归扫描 `SpellConfig.Cost`、`SpellConfig.TargetSelector`、`BuffConfig.Effects` 中的行为树节点。
4. 遇到 `BTCreateSpell.SpellConfigId` 时，把目标技能加入 `SpellConfig 表`，再继续展开目标技能的 Buff 和行为树。
5. 遇到 `BTAddBuff.ConfigId` 时，把目标 Buff 加入 `BuffConfig 表`，再继续展开目标 Buff 的 Effects。
6. 使用 visited 集合防止循环引用导致死循环。

来源说明以行级信息展示，例如：

- 主技能
- 来自 `SpellConfig.BuffId`
- 来自 `BTCreateSpell.SpellConfigId`
- 来自 `BTAddBuff.ConfigId`

点击来源说明时，尽量定位到对应行为树入口或相关资产。

## 增删改规则

新建主技能时，创建新目录：

```text
Packages/cn.etetet.statesync/Assets/BT/<主技能Id>-<Name>/
```

并创建：

- 主 `SpellScriptableObject`，文件名为主技能 Id。
- 主技能对应 `BuffScriptableObject`，默认 `BuffId = SpellId + 100000`，文件名为 BuffId。

新建子技能时，在当前主技能目录下创建子 `SpellScriptableObject`。默认 Id 使用同一组主技能 Id 的下一个可用子 Id，例如 `100010` 的子技能从 `100011`、`100012` 往后找。默认 Buff 仍按 `子技能Id + 100000` 创建。

新建 Buff 时，在当前主技能目录下创建 `BuffScriptableObject`。Id 使用当前链路附近下一个可用 BuffId，创建后由用户在行为树节点或字段中引用。

复制链时，复制当前主技能目录下被链路引用到的 Spell/Buff 资产，生成新 Id，并同步复制后资产内部的 `SpellConfigId`、`ConfigId`、`BuffId` 等引用。

删除时，先检查当前链路外是否仍引用目标资产。若存在外部引用，阻止删除并显示引用来源。若没有外部引用，允许删除资产，并清理当前链路内引用或提示用户修复。

重命名或改 Id 时，资产文件名必须与配置 Id 一致。编辑器负责重命名 asset 文件，并同步：

- `SpellConfig.Id`
- `SpellConfig.BuffId`
- `BuffConfig.Id`
- 行为树节点中的 `SpellConfigId`
- 行为树节点中的 `ConfigId`

## 校验规则

编辑器需要在刷新、保存、导出前运行校验：

- 资产名必须等于配置 Id。
- `SpellConfig.BuffId` 必须能找到 Buff。
- `BTCreateSpell.SpellConfigId` 必须能找到 Spell。
- `BTAddBuff.ConfigId` 必须能找到 Buff。
- 主技能 Id 必须满足 `% 10 == 0`。
- 子技能 Id 必须满足 `% 10 != 0`。
- 同 Id 重复资产标红。
- 行为树输入输出校验复用现有 `BTNodeValidator`。

导出按钮先保存资产，再调用现有导出器。导出失败时不吞异常，并在表格中保留校验错误信息。

## 模块拆分

实现阶段建议拆成以下类：

- `SpellEditorWindow`：窗口、工具栏、两个表。
- `SpellEditorAssetIndex`：扫描 Spell/Buff 资产并建立索引。
- `SpellEditorGraphBuilder`：从主技能递归构建技能链和 Buff 链。
- `SpellEditorReferenceScanner`：扫描行为树节点中的 `SpellConfigId`、`ConfigId` 等引用字段。
- `SpellEditorAssetOperations`：新建、复制、删除、重命名、改 Id、保存。
- `SpellEditorValidator`：重复 Id、缺失引用、命名、主子技能规则、行为树校验。

## 验证计划

- 使用项目唯一编译入口：

```powershell
dotnet build ET.sln
```

- 如果 UnityBridge 可用，再触发 Unity 刷新或编译，确认 Unity Editor 编译通过。
- 手工验证：
  - 打开 `ET/Spell/Spell Editor`。
  - 选择一个主技能。
  - `SpellConfig 表` 与 `BuffConfig 表` 显示完整链路。
  - 修改普通字段并保存。
  - 打开复杂字段对应的 BehaviorTreeEditor。
  - 新建、复制、删除、改 Id 时能维护资产名与引用。
  - 导出配置后生成现有 `CodeMode/Config` 文件。
