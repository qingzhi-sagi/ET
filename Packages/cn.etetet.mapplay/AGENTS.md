# cn.etetet.mapplay

地图玩法层包。

- 依赖 `cn.etetet.map` 基础包。
- 承接地图场景初始化、客户端地图表现、地图内玩法编排。
- 承接 UnitInfo、客户端/服务端 Unit 组装、地图内玩法消息与表现事件。
- 不直接依赖 `cn.etetet.transfer`，传送业务包需要玩法组装时由 `transfer` 依赖本包。
- 可以依赖 `spell`、`item`、`quest`、`login`、`move` 等玩法包。
- 不允许被 `cn.etetet.map` 反向依赖。
