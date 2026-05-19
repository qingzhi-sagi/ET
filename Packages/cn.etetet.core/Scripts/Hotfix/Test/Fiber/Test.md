# FiberId 统一编码测试计划

## 目标

- `Fiber.Zone` 必须从 `Fiber.Id` 解码得到。
- 显式 zone 创建入口统一为 `CreateFiber(zone, ...)`。
- `Fiber.LocalSlot` 必须从 `Fiber.Id` 解码得到。
- `FiberIdHelper` 必须按 32 bit zone + 32 bit localSlot 编码到 `long`。
- 同一 zone 下 Fiber 销毁后再次创建，不能复用旧 `FiberId`。

## 用例

### Core_Fiber_IdEncoding_Test

1. 创建独立测试 Fiber。
2. 使用 `CreateFiber(parent.Zone, ...)` 创建子 Fiber。
3. 验证子 Fiber 的 `Zone` 等于创建参数，且 `FiberIdHelper.DecodeZone(child.Id)` 一致。
4. 验证子 Fiber 的 `LocalSlot` 等于 `FiberIdHelper.DecodeLocalSlot(child.Id)`，并位于动态区间。
5. 销毁该子 Fiber。
6. 在同一 zone 再创建一个子 Fiber。
7. 验证新 FiberId 与旧 FiberId 不相同，且新 `LocalSlot` 大于旧 `LocalSlot`。

### Core_Fiber_IdEncoding32_Test

1. 使用大于旧 12 bit zone 上限的 zone 编码。
2. 使用大于旧 20 bit localSlot 上限的 localSlot 编码。
3. 验证编码结果超过 `int.MaxValue`。
4. 验证解码后的 zone 与 localSlot 与输入一致。
5. 验证 `int.MaxValue` 边界值可以完整往返。

### Core_FiberInstanceIdHeader_Test

1. 验证内网消息头中的 `FiberInstanceId` 长度为 16 字节。
2. 使用超过 `int.MaxValue` 的 Fiber 与 InstanceId 写入字节数组。
3. 验证按两个 `long` 读回的值与输入一致。
