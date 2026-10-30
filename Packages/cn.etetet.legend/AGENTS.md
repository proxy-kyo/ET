# cn.etetet.legend

## 规则

- 继承 `Packages/cn.etetet.harness/AGENTS.md` 的全部规则。
- 调整范围前读项目根 `docs/legend-plan.md`；确认术语时读根 `GLOSSARY.md`；改变既有设计决策前读根 `docs/adr/`。
- 登录复用 `cn.etetet.login`，登录地址读取 `GlobalConfig.Address`，账号和密码由用户输入。
- 入口使用 SceneType.Legend；地图纤程使用框架 SceneType.Map，客户端主纤程使用框架 SceneType.Client；登录界面为本包 LegendLogin。
- 依赖仅限本包 package.json 声明的保留包及其递归闭包；不引用 statesync、transfer、mapmanager、mapplay、move、spell、quest、item、btnode、robot 等非保留包类型。
- Gate 收客户端消息的入口 `NetComponentOnReadInvoker_Gate` 由 login 包提供；lockstep、kdemo 不进入依赖闭包。
- 地图位置变化复用 map 包的 ChangePosition_NotifyAOI；客户端 MailBoxType.LegendClient 保留进图前消息缓冲。
- 当前范围是登录进图、格子点击行走、AOI 单位互见与方块插值表现；UnitId 等于 PlayerId。
- 地图暂为 LegendMapConst 定义的 64×64 全可走空图，Tiled 接入后替换；服务端权威，不做本机预测。
- Unit 先添加 NumericComponent（AOI 使用毫米、Phase 为 Normal），再添加 GridMoveComponent 并 Enter，最后添加 AOIEntity；移除 Unit 前先移除 AOIEntity，保证离开视野事件处理时 Unit 仍存活。
- 移动统一调用 grid；不按 UnitType 分支，客户端消息不带单位类型，有无客户端只检查 UnitGateInfoComponent。遵守 `docs/adr/0005-natives-server-ai-indistinguishable.md`。
- LegendClient 使用自己的消息邮箱；进图 RPC 返回前的视野/步进消息暂存，CurrentScene 及视图初始化后按序转发。
- 客户端只发布 LegendUnitAppear，使用 LegendUnitViewComponent；不发布 AfterUnitCreate，不用 GameObjectComponent 或 M2C_CreateUnits/M2C_RemoveUnits。
- Entity 只放数据，逻辑放 System/Helper；await 后通过 EntityRef 重新获取实体；一类一文件。
- `.meta` 和 `AssemblyReference.asmref` 由工具生成，工程文件由 Unity 生成。

## 最小验证

1. 运行 `dotnet ./Bin/ET.Proto2CS.dll`；保存本次 Legend 导出结果，执行 `git restore Packages/cn.etetet.proto/CodeMode/`，写回 Legend 导出结果，确认其他包及 Grid 生成文件未变。
2. UnityBridge 先 Ping，再依次 AssetRefreshRequest（ForceUpdate）、ET/Init/CodeMode、RegenProject、Compile，等待每步成功；不手写工程文件。
3. 唯一编译入口 `dotnet build ET.sln`，仅允许已知的两个 NU1902；现有告警被视为错误时，用 `dotnet build ET.sln -p:WarningsNotAsErrors=NU1902%3BCS0618` 追加 NU1902 豁免并保留项目原有的 CS0618 豁免。
4. `"Test --Name=Grid.*" | dotnet ./Bin/ET.App.dll --SceneName=Test` 六项通过；同入口运行 `Legend_MovementIntegration.*`，验证出生占位、Phase、自身与他人可见、跨 AOI 格移动和移除。
5. ClientServer + Localhost 进入 Play 后立即用 kyo / 111111 登录，确认自己的方块、点击行走与相机跟随；双客户端确认互见与远端插值，离开视野移除。服务发现超时则退出 Play 重进。
6. 截图并检查本次 Play 的 Editor.log 与服务端日志无异常；最后 ExitPlay。未完成的交互验证必须明确报告。
