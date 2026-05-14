# Spell 与 Buff Id 合一设计

## 背景

当前 Spell/Buff 配置源来自 `Packages/cn.etetet.statesync/Assets/BT/**` 下的 `SpellScriptableObject` 与 `BuffScriptableObject`。现有规则里 `SpellConfig` 保存独立的 `BuffId` 字段，主 Buff 通常使用 `SpellId + 100000`，例如 `SpellConfig.Id = 100000` 引用 `BuffConfig.Id = 200000`。

现在希望主 Buff Id 与技能 Id 一致，并且删除 `SpellConfig.BuffId` 字段。这样会让同一目录下的 Spell 与 Buff 资产出现同 Id，因此不能继续使用纯数字文件名，需要把资产文件名改为带类型前缀：

```text
Spell100000.asset
Buff100000.asset
```

## 目标

- 删除 `SpellConfig.BuffId` 字段。
- 约定每个 Spell 的主 Buff 固定为同 Id 的 `BuffConfig`。
- ScriptableObject 资产文件名改为 `Spell{Id}.asset` 与 `Buff{Id}.asset`。
- 迁移现有资产时保留 `.meta` GUID，不破坏 Unity 引用。
- 更新编辑器、导出、运行时代码和测试中对旧 `BuffId` 字段或旧 `+100000` 编码规则的依赖。

## 非目标

- 不修改 Luban Excel 表结构。
- 不迁移 Spell/Buff 资产目录，仍使用 `Packages/cn.etetet.statesync/Assets/BT/**`。
- 不手工新建 `.meta` 文件。
- 不修改 `.csproj`。
- 不强制把所有独立 Buff 绑定到 Spell；只有 Spell 的主 Buff 采用同 Id 约定。

## 命名规则

Spell 资产名必须是：

```text
Spell{Id}
```

Buff 资产名必须是：

```text
Buff{Id}
```

例如：

```text
Packages/cn.etetet.statesync/Assets/BT/100000-AutoAttack/Spell100000.asset
Packages/cn.etetet.statesync/Assets/BT/100000-AutoAttack/Buff100000.asset
```

`SpellScriptableObject.OnValidate()` 与 `BuffScriptableObject.OnValidate()` 不再直接 `int.Parse(this.name)`，而是按类型前缀解析 Id。实现阶段可以临时兼容旧纯数字名，用于迁移前后过渡，但新建和校验只接受新命名。

## 数据模型

`SpellConfig` 删除：

```csharp
public int BuffId;
```

主 Buff Id 的唯一规则是：

```csharp
primaryBuffId = spellConfig.Id;
```

为了避免旧语义继续扩散，运行时和编辑器不再创建 `DefaultBuffId(spellId) = spellId + 100000` 这种入口。必要时只保留类型化命名 helper，例如：

```csharp
SpellAssetName(id) => $"Spell{id}"
BuffAssetName(id) => $"Buff{id}"
```

## 编辑器行为

`SpellEditorAssetIndex` 继续通过 ScriptableObject 类型建立索引：

- `SpellConfig.Id -> SpellScriptableObject`
- `BuffConfig.Id -> BuffScriptableObject`

`SpellEditorGraphBuilder` 展开 Spell 时，不再读取 `config.BuffId`，而是加入同 Id 的主 Buff：

```text
Spell 100000 -> Buff 100000
```

`SpellEditorWindow` 删除 `BuffId` 列。新建主技能或子技能时，同时创建：

```text
Spell{spellId}.asset
Buff{spellId}.asset
```

新建独立 Buff 时仍允许用户指定任意未占用的 Buff Id，文件名为 `Buff{Id}.asset`，但它不会成为某个 Spell 的主 Buff，除非 Id 与某个 Spell Id 相同。

复制技能链时：

1. Spell Id 按现有主技能组规则映射。
2. 被复制 Spell 的主 Buff 跟随 Spell 映射到同 Id。
3. 行为树中的 `BTCreateSpell.SpellConfigId` 按 Spell 映射更新。
4. 行为树中的 `BTAddBuff.ConfigId` 如果引用的是被复制的 Buff，则按 Buff 映射更新。

## 运行时行为

`SpellHelper.Cast()` 创建主 Buff 时使用 `spellConfig.Id` 作为 Buff 配置 Id：

```csharp
BuffHelper.CreateBuffWithoutInit(unit, unit.Id, IdGenerater.Instance.GenerateId(), spellConfig.Id, parent);
```

`Buff.GetSpellConfigId()` 对主 Buff 直接返回 `self.ConfigId`。该方法只适用于由 Spell 创建的主 Buff，独立 Buff 不应依赖它。现有调用点主要用于伤害结算中的 Spell Mod 查询，需要确保传入的是技能伤害 Buff。

如果后续需要区分独立 Buff 与主 Buff，可以新增 `TryGetSpellConfig()`，通过 `SpellConfigCategory.Contain(self.ConfigId)` 判断是否存在同 Id Spell。

## 导出链路

`ExportScriptableObjectEditor.ExportScriptableObject()` 的命名校验从纯数字改为类型化命名：

- Spell：`scriptableObject.name == $"Spell{SpellConfig.Id}"`
- Buff：`scriptableObject.name == $"Buff{BuffConfig.Id}"`

生成器 `BTConfigCodeExporter` 使用对象字段反射生成配置代码。删除 `SpellConfig.BuffId` 后，重新导出即可让 `CodeMode/Config/**/SpellConfigCategoryFactory_Config.cs` 不再生成 `BuffId = ...`。

## 迁移策略

迁移时需要先基于旧字段建立映射：

```text
oldSpellId -> oldBuffId
oldBuffId -> newBuffId = oldSpellId
```

对每个 Spell：

1. 读取旧 `SpellConfig.Id` 与旧 `SpellConfig.BuffId`。
2. 找到旧主 Buff 资产。
3. 把旧主 Buff 的 `BuffConfig.Id` 改成 Spell Id。
4. 把 Spell 资产文件名改成 `Spell{SpellId}.asset`。
5. 把 Buff 资产文件名改成 `Buff{SpellId}.asset`。
6. 保留对应 `.meta` 文件并同步重命名 `.meta`，确保 GUID 不变。

对行为树引用：

- `BTCreateSpell.SpellConfigId` 保持或按复制映射更新。
- `BTAddBuff.ConfigId` 如果命中旧主 Buff Id，则改为对应 Spell Id。
- 未命中旧主 Buff 映射的独立 Buff 引用保持原 Id。

对独立 Buff：

- Id 不变。
- 文件名从 `{Id}.asset` 改为 `Buff{Id}.asset`。

## 校验规则

刷新、保存和导出前应校验：

- Spell 资产名必须等于 `Spell{SpellConfig.Id}`。
- Buff 资产名必须等于 `Buff{BuffConfig.Id}`。
- 每个 Spell 必须存在同 Id 的主 Buff。
- `BTCreateSpell.SpellConfigId` 必须能找到 Spell。
- `BTAddBuff.ConfigId` 必须能找到 Buff。
- 主技能 Id 必须满足 `% 10 == 0`。
- 子技能 Id 通常满足 `% 10 != 0`。
- Spell Id 重复或 Buff Id 重复必须报错。
- 行为树输入输出校验继续复用现有 `BTNodeValidator`。

## 测试与验证

实现完成后使用项目唯一编译入口：

```powershell
dotnet build ET.sln
```

同时需要验证：

- Unity 中 `ET/Spell/Spell Editor` 能正常打开并刷新索引。
- 现有 Spell/Buff 资产迁移后没有丢失 `.meta` GUID。
- 导出配置后 `CodeMode/Config/**/SpellConfigCategoryFactory_Config.cs` 不再包含 `BuffId`。
- 服务器施法时主 Buff 使用同 Id 的 `BuffConfig`。
- `Spell_FiberSingleton_Test` 与 `Test_ConfigLoader_CodeConfigReload_Test` 不再依赖 `SpellConfig.BuffId`。

## 风险

- 迁移如果直接删除再创建资产，会导致 `.meta` GUID 变化；必须通过重命名保留 GUID。
- 旧 `OnValidate()` 会从纯数字文件名写回 Id，必须先改解析逻辑再做批量资产重命名。
- 行为树里 `BTAddBuff.ConfigId` 既可能引用主 Buff，也可能引用独立 Buff，迁移时只能改命中旧主 Buff 映射的引用。
- `Buff.GetSpellConfigId()` 旧实现依赖 `ConfigId / 10 * 10 - 100000`，必须移除，否则 Buff/Spell 同 Id 后伤害结算会读错 Spell。
