# cn.etetet.btnode

行为树节点扩展包。

- 可以依赖 `mapplay`、`map`、`move`、`spell`、`numeric`、`unit` 等行为节点运行所需包。
- 调用其它包的 Helper/System 方法前，必须在 `package.json` 中显式声明依赖。
- 不要让被依赖包反向依赖 `cn.etetet.btnode`，避免包循环。
- 行为节点只承载行为树节点逻辑，不把通用移动、地图、战斗基础能力写回本包。
