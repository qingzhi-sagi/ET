# ConditionExpr 使用说明

`cn.etetet.conditionexpr` 用于把 Excel/Luban 中填写的条件表达式编译成行为树，并在运行时返回条件检查结果。

典型用途：

```text
进入副本条件
领取奖励条件
任务接取条件
功能开启条件
```

## Luban 字段

本包定义了 Luban bean：

```text
ET.ConditionExpr
```

字段含义：

```text
Expr       string    条件表达式
ErrorCode  int       叶子条件没有显式错误码时使用
Desc       string    策划备注，不参与运行时判断
```

业务表中可以直接使用：

```text
##var     EnterCondition
##type    ET.ConditionExpr
##comment 进入条件
```

填写示例：

```text
Expr       = (HP >= 10 : 10001 || MP >= 100 : 10002) && Speed > 0 : 10003
ErrorCode  = 9999
Desc       = 进入副本条件
```

配置对象创建时，Luban 生成类会调用 `ConditionExpr.EndInit()`，表达式会被编译并保存到 `ConditionExpr.Root`。

## 表达式语法

支持的比较运算：

```text
>
>=
<
<=
==
!=
```

支持的逻辑运算：

```text
&&
||
!
()
```

错误码绑定：

```text
HP >= 10 : 10001
```

如果叶子条件没有写 `: errorCode`，会使用 `ET.ConditionExpr.ErrorCode`：

```text
HP >= 10
```

当 `BTEnv` 中有多个 owner 时，可以用 `OwnerKey.Variable` 指定读取哪个 owner。目标节点必须声明 public string `OwnerKey` 字段，否则编译表达式时报错。当前 `BTNumericCompareHandler` 仍按现有 `NumericComponent` 契约读取 `Unit`：

```text
Unit1.HP > 0 || Unit2.MP < 100
```

其中 `Unit1`、`Unit2` 是调用方传入 `BTEnv` 的 owner key，`HP`、`MP` 仍然来自 `NumericType` 常量名，并由 `BTNumericCompare.OwnerKey` 接收 owner key。

专用条件节点可以使用括号传字符串参数：

```text
Friend1(HP) > 0
Friend2(HP, MP) < 100
```

这里 `Friend1`、`Friend2` 是通过 `ConditionVariableAttribute` 注册的节点名，`HP`、`MP` 会按文本写入节点的 public `string[] Params` 字段。只有声明了 `Params` 字段的节点才能使用括号参数，`HP(Friend1)` 这类数值节点参数写法会在编译表达式时报错。

运算优先级：

```text
!  >  &&  >  ||
```

建议复杂条件显式加括号：

```text
(HP >= 10 : 10001 || MP >= 100 : 10002) && Speed > 0 : 10003
```

## 返回规则

行为树返回值约定：

```text
0     条件成功
非 0  条件失败，值为错误码
```

`BTSequence` 对应 `&&`：

```text
从左到右执行
遇到第一个失败节点，直接返回该节点错误码
全部成功返回 0
```

`BTSelector` 对应 `||`：

```text
从左到右执行
任意子节点成功，返回 0
全部失败时，返回第一个失败节点的错误码
```

`BTNot` 对应 `!`：

```text
子节点失败时，! 成功，返回 0
子节点成功时，! 失败，返回 BTNot.ErrorCode
```

示例：

```text
(HP >= 10 : 10001 || MP >= 100 : 10002) && Speed > 0 : 10003
```

返回含义：

```text
HP 满足且 Speed 满足          -> 0
HP 不满足但 MP 满足且 Speed 满足 -> 0
HP 和 MP 都不满足             -> 10001
HP 或 MP 满足但 Speed 不满足   -> 10003
```

## 变量注册

普通数值变量来自 `NumericType`。

`ConditionVariableRegistry` 初始化时会扫描 `NumericType` 的 public static int 字段，并注册：

```text
HP    -> BTNumericCompare, NumericType.HP
MP    -> BTNumericCompare, NumericType.MP
Speed -> BTNumericCompare, NumericType.Speed
```

因此表达式中的变量名必须和 `NumericType` 常量名一致：

```text
HP >= 10
MP >= 100
Speed > 0
Unit1.HP > 0
Unit2.MP < 100
```

如果变量没有注册，编译时会抛出：

```text
condition variable not registered: Xxx
```

## 普通数值比较

普通数值条件会编译成 `BTNumericCompare`。

运行时读取：

```csharp
Unit unit = env.GetEntity<Unit>(node.OwnerKey);
NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
long value = numericComponent.GetAsLong(node.NumericType);
```

所以调用方必须在 `BTEnv` 中传入表达式使用的 owner key。当前数值比较 Handler 按 `Unit` 读取，并要求对应 `Unit` 有 `NumericComponent`。

裸变量会默认读取：

```csharp
env.AddEntity(ConditionExprEnvKeys.Unit, unit);
```

带 owner key 变量读取表达式前缀对应的 key：

```csharp
env.AddEntity("Unit1", unit1);
env.AddEntity("Unit2", unit2);
```

## 专用变量节点

如果某个变量不能从 `NumericComponent` 读取，可以通过 `ConditionVariableAttribute` 注册专用节点。

示例：

```csharp
[ConditionVariable("VipLevel")]
public class BTVipLevelCompare : BTCondition
{
    public string[] Params;
    public ConditionCompareOp Op;
    public long Value;
    public int ErrorCode;
}
```

注册规则：

```text
专用节点必须继承 BTCondition
如果要接收表达式中的比较符、目标值和错误码，需要声明 Op / Value / ErrorCode public 字段
如果要支持 `OwnerKey.Variable` 语法，需要声明 OwnerKey public 字段，类型必须是 string
如果要支持 `NodeName(Param1, Param2)` 语法，需要声明 Params public 字段，类型必须是 string[]
变量名不能和已有 NumericType 或其它 ConditionVariableAttribute 重复
注册结果保存在 ConditionVariableRegistry 实例字段中，不使用静态字典
```

`BTNumericCompare` 和 `BTVipLevelCompare` 是平行关系，都继承 `BTCondition`。专用节点的 Handler 负责按业务规则取值并比较。

## 运行时调用

配置加载完成后，直接使用 Luban 对象里的 `Root`：

```csharp
ConditionExpr condition = mapConfig.EnterCondition;

BTEnv env = BTEnv.Create(scene, unit.Id);
try
{
    env.AddEntity(ConditionExprEnvKeys.Unit, unit);

    int errorCode = BTHelper.RunTree(condition.Root, env);
    if (errorCode != ErrorCode.ERR_Success)
    {
        return errorCode;
    }
}
finally
{
    env.Dispose();
}
```

也可以直接编译字符串：

```csharp
ConditionRoot root = ConditionExprCompiler.Compile(
    "(HP >= 10 : 10001 || MP >= 100 : 10002) && Speed > 0 : 10003",
    9999);
```

多 owner key 示例：

```csharp
ConditionRoot root = ConditionExprCompiler.Compile(
    "Unit1.HP > 0 || Unit2.MP < 100",
    9999);

BTEnv env = BTEnv.Create(scene, unit1.Id);
try
{
    env.AddEntity("Unit1", unit1);
    env.AddEntity("Unit2", unit2);

    int errorCode = BTHelper.RunTree(root, env);
}
finally
{
    env.Dispose();
}
```

## 测试

本包测试放在：

```text
Packages/cn.etetet.conditionexpr/Scripts/Hotfix/Test
```

执行：

```powershell
dotnet build ET.sln
Remove-Item ./Logs -Recurse -Force -ErrorAction SilentlyContinue
"Test --Name=Conditionexpr" | dotnet ./Bin/ET.App.dll --SceneName=Test
```

覆盖点：

```text
表达式编译结构
Luban bean EndInit 编译
NumericType 注册到 BTNumericCompare
运行时错误码返回
多 owner key 数值读取
专用节点 Params 参数解析
```
