# cn.etetet.unitybridge

## 概述

Unity 本地文件桥接包，提供：

- `DotNet~` 下的纯命令行 `ET.UnityBridge`
- `Editor` 下的 Unity Editor 文件宿主
- `Scripts/Model/Share` 下的桥接命令、错误码与共享文件协议

## 详细文档

- **架构规范**：请查看 `/et-arch` skill
- **编译构建**：请查看 `/et-build` skill
- **UnityBridge 命令调用**：请查看 `/et-unitybridge` skill

## 核心目录

| 路径 | 说明 |
|------|------|
| `DotNet~` | `ET.UnityBridge.csproj` 与命令行入口 |
| `Editor` | Unity Editor 宿主、处理器与分发逻辑 |
| `Scripts/Model/Share` | 桥接命令、错误码、路径与文件存储协议 |
