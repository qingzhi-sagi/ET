# cn.etetet.transfer

传送业务包。

- 承载地图传送编排、客户端切场景、传送协程锁、传送规则表与传送协议源文件。
- 本包是上层传送业务包，可以依赖 `mapplay` 并调用地图玩法的 Unit 组装逻辑。
- 客户端切场景事件类型与发布点归本包；具体 UI/表现订阅放在上层表现包（如 `statesync`），由上层包显式依赖本包。
- Gate 的 `C2G_EnterMapHandler` 属于 `login`，不放在本包；login 只创建临时 Map Fiber 并发送内部进图消息。
- `robot`、`test`、`statesync` 等需要传送能力的包应显式依赖本包。
