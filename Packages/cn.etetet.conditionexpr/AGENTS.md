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
