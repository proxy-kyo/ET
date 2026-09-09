# cn.etetet.kdemo

## 规则

- 继承 `Packages/cn.etetet.harness/AGENTS.md` 的全部规则。
- KDEMO 直接复用 statesync 的 YIUI `LoginPanelComponent` 与登录 prefab，不复制生成代码或资源。
- 登录复用 `cn.etetet.login`；不要复制登录协议、Realm Handler 或 Gate Handler。
- 登录地址读取 `GlobalConfig.Address`，账号和密码由用户输入，禁止硬编码。
- 当前最小范围只完成登录并取得 `PlayerId`，不包含进入地图。

## 最小验证

1. 使用唯一命令 `dotnet build ET.sln` 编译成功。
2. 以 `ClientServer + Localhost` 运行 KDEMO，YIUI 登录界面能够发起登录。
3. 登录成功后关闭登录界面，并在日志中输出非零 `PlayerId`。
5. `Logs/All.log` 不出现 KDEMO 登录相关异常。
