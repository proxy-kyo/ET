# legend 项目对齐结论

原创的 2000 年代韩系风格 MMORPG（固定斜视角、爆装备、PK、行会、攻城）。不照搬任何现有游戏的地图、职业名、物品、怪物、数值或 UI。

术语见 `GLOSSARY.md`，关键决策见 `docs/adr/`。

| 方面 | 决定 |
|---|---|
| 规模 | 单世界，50–300 人在线，单进程部署（Realm/Gate/Map/Location 都作为 Fiber 跑在同一个进程里） |
| 团队 | 一个人开发，素材尽量程序生成或 AI 生成，资源按 ID 引用，以后可以替换 |
| 框架 | ET10，在 `legend` 分支上开发，Unity 2022.3.62f3c1 |
| 游戏包 | 新建 `cn.etetet.legend`，作为第 5 层入口。登录参照 `cn.etetet.kdemo` 的做法复用 `cn.etetet.login`，`kdemo` 本身不改 |
| 移动 | 新写格子移动包：8 方向单步移动、一格只站一个单位、A* 寻路、服务端权威。替代 `move` 和 `recast` |
| 复用的 ET 包 | login、db、aoi、map、unit、numeric、item、equipment、spell、behaviortree。开工前逐个读源码确认能复用到什么程度 |
| 网络 | ET 默认：经 Router 走 KCP |
| 存储 | 只用 MongoDB（WSL Docker） |
| 配置 | Luban Excel |
| 热更 | HybridCLR 加 YooAssets，服务端热重载 |
| UI | YIUI，原生分辨率，不做像素化 |
| 画面 | 3D 模型加像素化渲染。内部分辨率 480×270（做成参数），手机固定高度、宽度随屏幕比例变。固定斜 45° 透视相机，可以小幅缩放，不能旋转 |
| 操作 | PC 点地移动、点怪攻击；手机虚拟摇杆加技能按钮，两端共用同一套逻辑 |
| 地图 | 用 Tiled 画格子图（地面、阻挡、传送点、刷怪区），客户端和服务端共用这一份数据 |
| 角色和外观 | 先用 Mixamo 人形模型加像素化渲染，装备外观先不做 |
| 平台 | 先做 Windows 和 Android，iOS 放后面，不做 WebGL |

## 第一个里程碑

登录 → 进入一张地图 → 走格子 → 打怪 → 掉落 → 拾取 → 多人互相可见。职业、PK、行会、攻城都放到后面。

## 原住居民

玩家降临到原住居民的世界，玩家才是外来者。原住居民由服务端 AI（behaviortree）驱动，自己打怪、升级、交易。对客户端来说，他们和真人玩家完全看不出区别。约束见 `docs/adr/0005-natives-server-ai-indistinguishable.md`。

## 已知风险

- URP 17.0.4（Unity 6 才用的版本）跑在 2022.3 上。之前 `packages-lock.json` 丢了 shadergraph 和 render-pipelines.core，第一次打开 Unity 时要检查依赖能不能正常解析。
