# et-git - ET Git 入口

## 何时使用

- 准备提交本次改动
- 检查 `git status`、`git diff`、`git diff --staged` 是否干净
- 筛除日志、临时文件、自动生成文件和无关改动
- 编写中文 `commit message`
- 拆分提交、整理暂存区、审查提交范围

## 不要加载

- 只是改代码、跑测试、导出配置，还没进入交付阶段
- 只是查看项目背景或架构规范

## 默认动作

1. 先看 `git status --short` 与 `git diff --stat`，确认影响范围。
2. 只保留本次任务相关改动，排除 `Logs/`、临时文件、无关自动生成文件和无关文件；如果本次任务需要提交 `.meta`、导出结果或生成代码，则保留它们。
3. 准备提交前，再看 `git diff` 与 `git diff --staged`，确认没有混入无关改动。
4. 提交信息统一使用**中文**，优先写成 `动作 + 对象 + 影响范围/原因`。
   - 例：`新增 Quest 任务完成检测逻辑`
   - 例：`修复 await 后 Entity 访问导致的空引用`
   - 例：`优化 UnityBridge 心跳重试策略`
5. 与远端同步时，默认优先使用 `git pull --rebase`，或先 `git fetch` 再 `git rebase`。
6. 若与远端产生冲突，**禁止使用 `merge`**，只能使用 `rebase` 处理。
7. 若涉及拆分提交、回滚、`cherry-pick`、`rebase` 等风险操作，先说明影响再执行。

## 预提交检查序列

```
git status --short         # 1. 查看所有变更文件
git diff --stat            # 2. 查看变更统计
git diff                   # 3. 逐行检查未暂存变更
git diff --staged          # 4. 逐行确认已暂存内容
```

## 需要排除的文件

- `Logs/` 目录（运行日志，每次启动都会变化）
- 与本次任务无关的 `*.meta` 文件；新建或移动 C# 文件、资源时要保留对应 `.meta`
- `Bin/` 目录下的编译输出
- 与本次任务无关的自动生成文件（`Luban/` 导出结果、`Proto/` 生成的 C# 文件等）
- IDE 临时文件（`.vs/`、`.idea/`、`*.user`）

## 优先入口命令

- `git status --short`
- `git diff --stat`
- `git diff`
- `git diff --staged`
- `git pull --rebase`
- `git fetch`
- `git rebase`
