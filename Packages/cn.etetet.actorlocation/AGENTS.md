# cn.etetet.actorlocation

## 包职责

- Actor Location 路由、锁、代理重试与 `MessageLocationSender`。
- Location 路由状态会持久化到 DB；修改持久化结构前必须设计迁移方案。

## 开发规则

- 修改本包代码前先遵守根目录 `AGENTS.md` 与 `Packages/cn.etetet.harness/AGENTS.md`。
- 所有命令必须使用 `pwsh`。
- 涉及 `ETTask` / `await` 后继续访问 Entity 时，必须使用 `EntityRef<T>` 重新获取。
- Location 锁必须使用 token 闭环：调用方使用 `LockWithToken` 获取 token，并调用带 `lockToken` 的 `UnLock`。
- 不要新增无 token 解锁路径；旧无 token API 只保留为编译期阻断入口。
- 不手工生成 `.meta`，不手工修改 `.csproj`。

## 测试入口

```powershell
dotnet build ET.sln
```

```powershell
"Test --Name=Actorlocation_.*" | dotnet ./Bin/ET.App.dll --SceneName=Test
```
