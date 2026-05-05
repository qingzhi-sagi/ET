# FiberId 统一编码设计

## 背景

当前 Fiber 创建逻辑同时存在显式 `zone` 字段、固定 `ConstFiberId`、`CreateFiberWithId`、`CreateZoneFiber` 等入口，`fiberId` 参数在不同路径中有时表示完整 FiberId，有时表示区内槽位。这会让 FiberId 语义不统一，也会影响并发测试。

并发测试的核心需求是：每个测试可以创建自己的 Realm、Gate、Location 等业务服务，并通过不同 zone 隔离。如果这些服务复用同一套配置结构，运行时 FiberId 必须仍然唯一。

## 目标

- `Fiber.Id` 成为唯一权威标识。
- `Fiber.Id` 始终由 `FiberIdHelper.Encode(zone, internalLocalSlot)` 生成。
- `Fiber.Zone` 从 `Fiber.Id` 解码得到，不再保存独立字段。
- 创建 Fiber 时业务只传 zone，不传 localSlot，也不传 encoded FiberId。
- localSlot 完全由 `FiberManager` 内部分配。
- 进程级基础设施 Fiber 共享，使用 `zone = 0`。
- 业务服务 Fiber 按业务 zone 创建，并通过 ServiceDiscovery 查找。

## 非目标

- 不使用 `StartSceneConfig.Id` 作为 Fiber localSlot。
- 不提供 `CreateFiberByEncodedId` 或类似直接传完整 FiberId 的创建入口。
- 不提供 `CreateProcessFiber`。进程级 Fiber 统一通过 `CreateFiber(0, ...)` 创建。
- 不把 Realm、Gate、Location、MapManager 等业务服务放入进程级 Fiber 地址注册表。

## 核心规则

```text
Fiber.Id = Encode(zone, internalLocalSlot)
Fiber.Zone = DecodeZone(Fiber.Id)
Fiber.LocalSlot = DecodeLocalSlot(Fiber.Id)
```

`Fiber.LocalSlot` 只作为框架诊断信息，不暴露为业务创建参数。

创建规则：

```text
业务 Fiber: CreateFiber(zone, ...)
当前 zone 子 Fiber: CreateFiber(...)
进程级基础设施 Fiber: CreateFiber(0, ...)
```

## Fiber API 设计

保留 `CreateFiber` 命名，但统一语义为“按 zone 创建 Fiber”。

建议入口：

```csharp
public async ETTask<Fiber> CreateFiber(int zone, long rootId, int sceneType, string name);

public async ETTask<int> CreateFiber(
    int zone,
    SchedulerType schedulerType,
    long rootId,
    int sceneType,
    string name);

public async ETTask<Fiber> CreateFiber(long rootId, int sceneType, string name);

public async ETTask<int> CreateFiber(
    SchedulerType schedulerType,
    long rootId,
    int sceneType,
    string name);
```

无 zone 参数的重载使用 `this.Zone`。

废弃或删除以下入口：

```text
CreateFiberWithId
CreateZoneFiber
CreateZoneFiberWithId
FiberManager.CreateFiber(int fiberId, ..., int zone, ...)
```

## Fiber 对象设计

`Fiber` 不再保存独立 zone 字段：

```csharp
public int Zone => FiberIdHelper.DecodeZone(this.Id);
public int LocalSlot => FiberIdHelper.DecodeLocalSlot(this.Id);
```

构造函数去掉 zone 参数：

```csharp
internal Fiber(
    int id,
    long rootId,
    int sceneType,
    string name,
    SchedulerType schedulerType,
    Fiber parent)
```

这样可以避免 `Fiber.Id` 与 `Fiber.Zone` 不一致。

## localSlot 分配

`FiberManager` 负责分配 localSlot。动态 localSlot 在进程生命周期内不回收、不复用，即使 Fiber 已经销毁也不把 localSlot 重新分配给新的 Fiber。

建议使用按 zone 分组的动态分配器：

```csharp
private readonly Dictionary<int, int> nextLocalSlotByZone = new();
```

每个 zone 的 localSlot 从动态区间开始：

```csharp
FiberIdHelper.DynamicLocalSlotStart
```

普通动态分配区间为：

```text
[FiberIdHelper.DynamicLocalSlotStart, FiberIdHelper.ReservedLocalSlotStart)
```

`ReservedLocalSlotStart` 及之后的槽位不参与普通动态分配。

分配流程：

```text
1. ValidateZone(zone)
2. 从 nextLocalSlotByZone[zone] 分配新槽位
3. 校验新槽位不能到达 ReservedLocalSlotStart
4. fiberId = Encode(zone, localSlot)
5. 创建 Fiber
```

同一进程中，zone 不同可以复用相同 localSlot 序列，最终 FiberId 仍不冲突。

不回收 localSlot 的原因是 FiberId 会出现在 `ActorId`、延迟消息、队列项、异步回调或外部缓存中。旧 Fiber 销毁后，如果新 Fiber 复用相同 FiberId，残留消息可能被错误投递到新 Fiber，导致更隐蔽的业务 bug。

达到 `ReservedLocalSlotStart` 时直接抛出明确异常，暴露当前 zone 过度创建 Fiber 的问题。长时间高频创建和销毁的对象不应默认建模为 Fiber；如果确实需要大量短生命周期执行单元，应改用 Entity、任务对象、组件池，或为测试分配新的 zone。

因此本设计用“进程内 FiberId 不复用”换取地址安全性。容量约束由 `LocalSlotBits` 决定，需要通过监控和异常信息及时发现。

## 进程级 Fiber 地址注册表

新增进程级 singleton，用于保存 NetInner、ServiceDiscoveryAgent 等基础设施 Fiber 的真实地址。注册表按 `SceneType` 查询，不使用硬编码字段。

建议位置：`cn.etetet.core`，因为它只依赖 `FiberInstanceId` 和 `SceneType` 的 int 值，不依赖具体业务包。

建议结构：

```csharp
public class ProcessFiberAddressSingleton : Singleton<ProcessFiberAddressSingleton>, ISingletonAwake
{
    private readonly Dictionary<int, FiberInstanceId> fiberInstanceIds = new();

    public void Register(int sceneType, FiberInstanceId fiberInstanceId);

    public FiberInstanceId Get(int sceneType);

    public bool TryGet(int sceneType, out FiberInstanceId fiberInstanceId);
}
```

注册规则：

- 同一个 `sceneType` 首次注册成功。
- 重复注册相同 `FiberInstanceId` 可以视为幂等。
- 重复注册不同 `FiberInstanceId` 应抛异常，防止进程级基础设施被意外覆盖。

使用方式：

```text
NetInner 创建完成 -> Register(SceneType.NetInner, new FiberInstanceId(netInner.Id))
ServiceDiscoveryAgent 创建完成 -> Register(SceneType.ServiceDiscoveryAgent, new FiberInstanceId(agent.Id))
MessageSenderSystem -> Get(SceneType.NetInner)
ServiceDiscoveryProxySystem -> Get(SceneType.ServiceDiscoveryAgent)
```

## 服务发现边界

进程级注册表只保存基础设施 Fiber：

```text
NetInner
ServiceDiscoveryAgent
```

业务服务继续走 ServiceDiscovery：

```text
Realm
Gate
Location
MapManager
Router
RouterManager
```

业务服务隔离依赖 metadata：

```csharp
ServiceMetaKey.Zone = root.Fiber.Zone.ToString()
```

查询时继续使用 zone 过滤，例如 `GetBySceneTypeAndZone(...)`。

ServiceDiscoveryAgent 是进程级 Fiber，不能再用自身 `Fiber.Zone` 表示业务 zone。业务 zone 必须来自服务注册 metadata 或查询 filter。

## 迁移范围

需要调整的关键位置：

1. `FiberIdHelper`
   - 保留 `Encode`、`DecodeZone`、`DecodeLocalSlot`、校验方法。
   - 明确 localSlot 是内部动态槽位。

2. `FiberManager`
   - 改为内部按 zone 分配 localSlot。
   - 删除直接传完整 FiberId 或手工 localSlot 创建 Fiber 的入口。

3. `Fiber`
   - 删除独立 `Zone` 字段。
   - `Zone`、`LocalSlot` 改为从 `Id` 解码。

4. 启动流程
   - NetInner 使用 `CreateFiber(0, ...)` 创建并注册到 `ProcessFiberAddressSingleton`。
   - ServiceDiscoveryAgent 使用 `CreateFiber(0, ...)` 创建并注册到 `ProcessFiberAddressSingleton`。
   - Realm、Gate、Location 等业务服务使用 `CreateFiber(startConfig.Zone, ...)` 创建。

5. `MessageSenderSystem`
   - 不再使用 `ConstFiberId.NetInnerFiberId`。
   - 改为从 `ProcessFiberAddressSingleton.Get(SceneType.NetInner)` 获取。

6. `ServiceDiscoveryProxySystem`
   - 不再通过 `ServiceDiscoveryFiberHelper.CreateAgentFiberInstanceId(zone)` 推导 Agent。
   - 改为从 `ProcessFiberAddressSingleton.Get(SceneType.ServiceDiscoveryAgent)` 获取。

7. `ServiceDiscoverySystem`
   - 不再通过 `DecodeZone(actorId.FiberInstanceId.Fiber)` 推导远端 AgentId。
   - 远端 Agent 地址应来自注册、心跳或显式消息字段。

8. 测试辅助代码
   - 不再通过 `FiberIdHelper.Encode(zone, localSlot)` 推导已创建 Fiber。
   - 改为使用创建返回值、`GetFiber(string name)` 或 ServiceDiscovery 查询。

9. `StartSceneConfig.GetActorId`
   - 不能再用 `StartSceneConfig.Id` 构造 `FiberInstanceId`。
   - 需要改为通过运行时已创建的 Fiber 或 ServiceDiscovery 获取真实 ActorId。

## 验证计划

基础验证：

```powershell
dotnet build ET.sln
```

重点行为验证：

- 并发测试创建多个 zone 的 Realm、Gate、Location 不发生 FiberId 冲突。
- `Fiber.Zone` 从 `Fiber.Id` 解码后与创建时传入 zone 一致。
- 同一 zone 创建、销毁、再创建 Fiber 时，不复用旧 FiberId。
- 动态 localSlot 达到上限时抛出明确异常，包含 zone 和 localSlot 分配信息。
- NetInner 能通过 `ProcessFiberAddressSingleton` 被 `MessageSenderSystem` 找到。
- ServiceDiscoveryProxy 能通过 `ProcessFiberAddressSingleton` 找到 ServiceDiscoveryAgent。
- Realm/Gate/Location 等业务服务仍通过 ServiceDiscovery 注册和查询。
- ServiceDiscovery 按 `ServiceMetaKey.Zone` 过滤结果正确。
- 代码中不再存在通过固定 `ConstFiberId` 或配置 id 推导业务 FiberId 的路径。

## 风险与约束

- 这是破坏性重构，必须一次性收口 Fiber 创建入口，否则 `Fiber.Zone` 从 `Id` 解码后会暴露历史不一致。
- ServiceDiscoveryAgent 改为进程级后，不能再用自身 zone 访问业务分区数据；需要按 metadata 或请求字段处理业务 zone。
- 测试中构造虚拟 `ActorId` 的 helper 需要重新区分“真实 Fiber 地址”和“测试伪造地址”。
- 动态 localSlot 不回收，长时间高频创建 Fiber 的同一 zone 可能耗尽 localSlot；这类场景需要重新评估是否应使用 Fiber。
- 如果某些场景确实需要稳定直连 Fiber，必须先证明不能通过 ServiceDiscovery、创建返回值或进程级注册表解决，再单独设计，不应恢复 localSlot 外部传参。
