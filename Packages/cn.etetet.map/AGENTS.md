# cn.etetet.map

地图基础包。

- 当前作为 `move` 依赖的地图基础能力包，不能反向依赖 `cn.etetet.move`。
- 承载地图消息广播、地图基础组件、地图基础配置与通用地图能力。
- 不要依赖 `mapplay`、`spell`、`item`、`quest` 等更高层玩法包。
- 跨包访问必须在 `package.json` 显式声明依赖，并避免循环依赖。
