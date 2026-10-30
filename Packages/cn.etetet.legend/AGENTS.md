# cn.etetet.legend

## 规则

- 继承 `Packages/cn.etetet.harness/AGENTS.md` 的全部规则。
- 调整范围前读项目根 `docs/legend-plan.md`；确认术语时读根 `GLOSSARY.md`；改变既有设计决策前读根 `docs/adr/`。
- 登录复用 `cn.etetet.login`，登录地址读取 `GlobalConfig.Address`，账号和密码由用户输入。
- 使用独立的 Legend、LegendClient、LegendMap SceneType；登录界面为本包 LegendLogin。
- 框架包之间有未声明的相互依赖（例如 map 引用 mapmanager，mapplay 引用 statesync 的 HPViewComponent），所以 package.json 里依赖了 statesync、transfer、mapmanager、mapplay、move、spell、quest、item、btnode、robot 等，它们会被编译。legend 只靠它们通过编译，**代码不引用它们的类型和方法**，进图走 legend 自己的协议，不走 transfer/mapmanager。
- Gate 收客户端消息的入口 `NetComponentOnReadInvoker_Gate` 用 statesync 那份，legend 不再自带。lockstep、kdemo 不进入依赖闭包。
- `SceneType.Client`、`SceneType.Map` 的处理器不会作用到 legend 的纤程，因为 legend 只用自己的 SceneType。
- 当前范围是登录后进入空地图并创建玩家 Unit；UnitId 等于 PlayerId。
- Entity 只放数据，逻辑放 System/Helper；await 后通过 EntityRef 重新获取实体；一类一文件。
- `.meta` 和 `AssemblyReference.asmref` 由工具生成，工程文件由 Unity 生成。

## 最小验证

1. 运行 `dotnet ./Bin/ET.Proto2CS.dll`，确认 CodeMode 生成 Legend 协议。
2. Unity 重新生成工程后，使用唯一命令 `dotnet build ET.sln` 编译；首次导入前不编译旧工程文件。
3. 以 `ClientServer + Localhost` 运行 Legend，登录成功后关闭面板，日志出现 `legend login succeeded. PlayerId:` 且 PlayerId 非零。
4. 客户端建立 CurrentScene，日志出现 `legend enter map succeeded. Map: LegendMap1 UnitId:`，UnitId 等于 PlayerId。
5. 检查 `Logs/All.log` 没有 Legend 登录或进图异常，并验证重复登录后地图中的玩家 Unit 被替换。
