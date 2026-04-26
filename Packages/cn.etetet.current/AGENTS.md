# cn.etetet.current

## 概述

Current 包用于承载客户端当前逻辑空间相关的底层抽象，例如 `SceneType.Current` 与当前场景消息标记接口。

## 使用规则

- 本包只放跨玩法通用的当前空间基础类型，不放具体地图、房间、Buff、Unit 等业务逻辑。
- 需要让消息投递到客户端当前场景时，优先使用 `ICurrentMessage` 标记消息类型。
- 具体 Handler 应放在所属业务包中。
