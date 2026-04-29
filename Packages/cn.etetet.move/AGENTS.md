# cn.etetet.move

移动包。

- 按当前项目约定，`move` 依赖 `map`，`map` 不反向依赖 `move`，避免包循环。
- 依赖只写 `move` 源码直接引用的包；不要把 `map` 的间接依赖递归展开到这里。
- 当前直接依赖为 `core`、`login`、`map`、`numeric`、`proto`、`recast`、`unit`。
- 不要依赖 `mapplay`、`spell`、`item`、`quest` 等更高层玩法包。
- 承载移动、停止、转向相关组件、系统、消息定义、客户端/服务端 Move handler。
- `Proto/Move_*.proto` 中的消息如需从其它包迁入，优先保持既有 opcode 兼容。
