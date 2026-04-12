# et-async - ET Async 入口

## 何时使用

- 需要新增、修改或 review `async` / `await` / `ETTask` / `ETTask<T>`
- 需要判断某段逻辑是否应该异步化，或是否应该改回同步
- 需要处理 `await` 后 `Entity` 访问、`EntityRef<T>`、Handler `Run` 异步安全
- 需要设计并发等待、收集协程、`ETTask.WaitAll(...)`
- 需要使用 `ETCancellationToken`、`NewContext(...)` 或异步上下文透传

## 不要加载

- 代码完全同步，且不涉及异步边界、安全或取消控制
- 只是编译、导出、跑测试、操作 Unity / Excel

## 默认动作

1. 先判断是否真的需要异步，并明确"谁在等待这个返回值"。
2. 只有确实要等待 RPC、IO、定时器、协程锁、跨 Fiber 结果或其它异步资源时，才保留 `async` / `ETTask`。
3. 调用方不依赖返回值时，优先改成同步方法、普通消息、`Send` 或独立协程，不要为了包一层转发保留异步签名。
4. 只要存在 `await` 后继续访问 `Entity` 的路径，就在 `await` 前创建 `EntityRef<T>`，并在 `await` 后重新获取实体。
5. 函数参数传 `Entity`，字段、属性、集合里保存 `EntityRef<T>`；不要直接长期持有 `Entity`，也不要使用 `.Entity` 访问。
6. 在哪消费结果，就在哪个协程里直接 `await`；不要设计"先返回一个 `ETTask` 句柄，后面再 await"的桥接层。
7. 需要并发时，为每一路启动独立收集协程，内部直接 `await`，外层只负责触发、`ETTask.WaitAll(...)` 和汇总。
8. `NewContext(...)` 只用于启动独立协程并传递 `ETCancellationToken`；不要把 `Scene`、`Unit` 等 `Entity` 当 context 透传，需要带实体时改用 `EntityRef<T>`。
9. Handler、测试协程、普通系统方法都遵守同一套异步安全规则，不因入口不同而放宽。

## EntityRef 在 async/await 下的正确写法

```csharp
// ✅ 正确：await 前创建 EntityRef，await 后重新获取
public static async ETTask ProcessUpdate(this UpdateCoordinatorComponent self, UpdateTask task)
{
    EntityRef<UpdateCoordinatorComponent> selfRef = self;
    EntityRef<UpdateTask> taskRef = task;

    await SomeAsyncOperation();

    // await 后必须通过 EntityRef 重新获取
    self = selfRef;
    task = taskRef;

    // 现在可以安全使用
    task.UpdateProgress("处理完成");
}

// ✅ 正确：字段存 EntityRef，不存 Entity
public class SomeComponent : Entity, IAwake
{
    public Dictionary<int, EntityRef<ProcessInfo>> ProcessDict { get; set; }
}

// ❌ 错误：await 后直接用旧变量
await SomeAsyncOperation();
self.DoSomething();  // self 可能已失效！

// ❌ 错误：用 .Entity 属性访问
var entity = processRef.Entity;  // 禁止！应直接赋值：entity = processRef;

// ❌ 错误：字段直接存 Entity
public List<ProcessInfo> Processes { get; set; }  // 应改为 List<EntityRef<ProcessInfo>>
```

## 易错点

- `await` 后直接访问旧实体变量
- 返回 `ETTask<ETTask<T>>`，或写 `RunXxx + ETTask.Create + SetResult` 这种中转封装
- 把 `Entity` 塞进 `NewContext(...)` 或等待结果对象
- 为了"接口统一"或"以后可能扩展"提前异步化
- Handler `Run` 方法含 `await` 时未在 `await` 后重新获取 Entity（Handler 没有豁免权）

## 输出要求

- 说明异步是否必要，以及谁在等待结果
- 说明 `await` 后的实体访问是否通过 `EntityRef<T>` 保护
- 说明并发组织方式是否避免了延后消费 `ETTask`
- 说明 `ETCancellationToken` / `NewContext(...)` 的使用是否只承担取消与协程边界职责
