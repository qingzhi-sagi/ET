# FiberId 统一编码测试计划

## 目标

- `Fiber.Zone` 必须从 `Fiber.Id` 解码得到。
- 显式 zone 创建入口统一为 `CreateFiber(zone, ...)`。
- `Fiber.LocalSlot` 必须从 `Fiber.Id` 解码得到。
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
