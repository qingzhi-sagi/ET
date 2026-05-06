# ET Git 参考

## 红线

- 禁止直接使用 `git pull`。
- 远端同步必须用 `git pull --rebase`，或 `git fetch` 后 `git rebase`。
- 若与远端冲突，只能继续 `rebase` 流程，禁止 `merge`。

## 提交前检查

```powershell
git status --short
git diff --stat
git diff
git diff --staged
```

- 先确认影响范围，再暂存文件。
- 提交前必须看 staged diff。
- 若任务涉及代码或测试，提交前先确认必要的 `et-build` / `et-test-run` 验证。

## 变更筛选

- 只提交本次任务相关文件。
- 排除 `Logs/`、`Bin/` 编译输出、临时文件、IDE 文件、无关自动生成文件。
- 新建或移动 C# / Unity 资源时，保留对应 `.meta`。
- Luban / Proto 生成结果只有本次任务确实需要时才提交。
- 若改动范围比预期大，先解释原因，再决定是否继续。

## 提交信息

- 提交信息统一中文。
- 推荐格式：`动作 + 对象 + 影响范围/原因`。
- 示例：`优化 ET skills 路由与参考文档`。
- 一个功能尽量对应一次提交，避免多个目标塞进同一条提交。

## 暂存区整理

- 需要精确控制范围时，先看 `git diff` 再暂存。
- 若暂存区混入无关文件，先整理后继续。
- 不确定文件归属时，输出检查结果和建议，不直接提交。

## 远端同步

```powershell
git pull --rebase
```

或：

```powershell
git fetch
git rebase
```

- rebase 前确认工作区和暂存区状态。
- rebase 冲突时，先解决冲突，再继续 `git rebase --continue`。
- 不使用 merge commit 整合远端。

## 风险操作

- `cherry-pick`、回滚、重写历史、批量暂存/反暂存、`rebase` 前，先说明影响范围。
- 若操作可能影响未提交改动，先明确风险再执行。
- 不确定时只输出检查结果与建议。
