# cn.etetet.conditionexpr

## 概述

ConditionExpr 包负责把 Luban/Excel 中填写的条件表达式编译成行为树节点，并在运行时通过 `BTEnv` 执行。

## 目录约定

- `Scripts/Model/Share`：表达式模型、编译器、BT 节点数据、包元数据。
- `Scripts/Hotfix/Share`：运行时 Handler。
- `Scripts/Hotfix/Test`：本包测试用例。
- `Luban/Config/Base`：ConditionExpr Luban bean 定义。

## 开发约定

- 只依赖已声明的底层包：`behaviortree`、`unit`、`numeric`。
- 不新增静态注册表或静态缓存；Attribute 扫描结果必须挂在 ET 生命周期对象上。
- 通用逻辑节点复用 `cn.etetet.behaviortree` 中的 `BTSequence`、`BTSelector`、`BTNot`。
- ConditionExpr 自己的节点定义在本包内，不放回 `cn.etetet.btnode`。
- 只给测试使用的 ConditionExpr 模型节点放在 `Scripts/Model/Test`，不要放到 `Scripts/Model/Share`。
- 变量支持 `OwnerKey.Variable` 语法，例如 `Unit1.HP > 0 || Unit2.MP < 100`；裸数值变量 `HP` 继续默认读取 `ConditionExprEnvKeys.Unit`。
- `OwnerKey.Variable` 只允许一层点号，`OwnerKey` 必须是调用方已通过 `BTEnv.AddEntity(key, owner)` 传入的 owner key，`Variable` 对应的节点必须声明 public string `OwnerKey` 字段；当前数值比较 Handler 仍按现有 `NumericComponent` 契约读取 `Unit`。
- 专用条件节点支持 `NodeName(Param1, Param2)` 语法，参数会写入该节点的 public `string[] Params` 字段；没有 `Params` 字段的节点不能使用括号参数，例如 `HP(Friend1)` 必须报错。
