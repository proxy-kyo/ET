# ET 框架消息传递时序图：用户登录

下面以客户端调用 `LoginHelper.Login` 为例，展示一次“登录 Realm，取得 Gate 登录票据，再登录 Gate”的完整消息链路。图中把网络传输、软路由、Session、消息分发和业务 Handler 分开表示。

```mermaid
sequenceDiagram
    autonumber
    participant Main as 客户端 Main/业务纤程
    participant NC as 客户端 NetClient 纤程\nNetComponent + Session
    participant Router as Router 软路由\nRouterComponent
    participant RealmNet as Realm NetComponent\nKCP/UDP 接入
    participant Realm as Realm 纤程\nNetComponentOnReadInvoker_Realm
    participant MD as MessageSessionDispatcher
    participant RealmLogin as C2R_LoginHandler
    participant Sender as Realm MessageSender\n内网 Actor 消息
    participant Gate as Gate 纤程\nR2G_GetLoginKeyHandler
    participant GateNet as Gate NetComponent\nKCP/UDP 接入
    participant GateLogin as C2G_LoginGateHandler
    participant Player as PlayerComponent / PlayerSessionComponent

    Main->>NC: Main2NetClient_Login(account, password, routerAddress)
    NC->>Router: RouterSYN(localConn, realmAddress)
    Router-->>NC: RouterACK / 建立路由节点
    NC->>RealmNet: CreateRouterSession 后发送 C2R_Login
    RealmNet->>Realm: 解码消息，生成 NetComponentOnRead
    Realm->>MD: ISessionMessage → Handle(session, C2R_Login)
    MD->>RealmLogin: 按 opcode + SceneType.Realm 调用 Run

    RealmLogin->>RealmLogin: ServiceDiscoveryProxy 选择同 Zone 的 Gate
    RealmLogin->>Sender: Call(Gate.ActorId, R2G_GetLoginKey{Account})
    Sender->>Gate: 内网 Actor/RPC 消息
    Gate->>Gate: MailBoxComponent / MessageDispatcher 分发
    Gate->>Gate: MessageHandler(SceneType.Gate)
    Gate->>Gate: R2G_GetLoginKeyHandler.Run
    Gate->>Gate: GateSessionKeyComponent.Add(key, account)
    Gate-->>Sender: G2R_GetLoginKey{Key, GateId}
    Sender-->>RealmLogin: 返回 RPC 响应
    RealmLogin-->>RealmNet: R2C_Login{Address, Key, GateId}
    RealmNet-->>NC: 经 Router 转发 R2C_Login
    NC->>NC: NetClientOnReadInvoker_NetClient\nIResponse → Session.OnResponse
    NC-->>Main: Login RPC 完成，取得 Gate 地址和 key

    NC->>Router: RouterSYN(localConn, gateAddress)
    Router-->>NC: RouterACK / 建立第二个路由节点
    NC->>GateNet: CreateRouterSession 后发送 C2G_LoginGate{Key, GateId}
    GateNet->>Gate: 解码消息，生成 NetComponentOnRead
    Gate->>MD: ISessionMessage → Handle(session, C2G_LoginGate)
    MD->>GateLogin: 按 opcode + SceneType.Gate 调用 Run
    GateLogin->>GateLogin: GateSessionKeyComponent.Get(Key)
    GateLogin->>Player: PlayerComponent.GetByAccount / Add Player
    GateLogin->>Player: 绑定 PlayerSessionComponent ↔ Session\n添加 MailBoxComponent(GateSession)
    GateLogin-->>GateNet: G2C_LoginGate{PlayerId}
    GateNet-->>NC: 经 Router 转发 G2C_LoginGate
    NC->>NC: IResponse → Session.OnResponse
    NC-->>Main: 登录 Gate 成功，返回 PlayerId
```

## 模块经过关系

| 阶段 | 主要模块 | 作用 |
| --- | --- | --- |
| 客户端入口 | `cn.etetet.login` / `Main2NetClient_LoginHandler` | 创建 Realm、Gate 两个 Session，串联登录步骤 |
| 软路由 | `cn.etetet.router` / `RouterComponent` | 将客户端外网连接映射到 Realm 或 Gate 的内网 KCP 连接 |
| 网络接入 | `cn.etetet.core` / `NetComponent`、`SessionSystem` | 收发字节、序列化消息、维护 RPC 回调 |
| Realm 入站 | `cn.etetet.login` / `NetComponentOnReadInvoker_Realm` | 将 `ISessionMessage` 交给会话消息分发器 |
| 会话分发 | `cn.etetet.core` / `MessageSessionDispatcher` | 依据 opcode 和场景类型选择 Handler |
| Realm 登录业务 | `C2R_LoginHandler` | 选择 Gate，并通过内网 RPC 请求登录 key |
| Realm→Gate 内网 | `MessageSender` + Gate `MessageDispatcher` | 按 Gate ActorId 投递 `R2G_GetLoginKey` |
| Gate 发票 | `R2G_GetLoginKeyHandler`、`GateSessionKeyComponent` | 生成临时 key 并记录 account 映射 |
| Gate 登录业务 | `C2G_LoginGateHandler` | 校验 key，创建/复用 Player，绑定 Gate Session |
| 客户端响应 | `NetClientOnReadInvoker_NetClient`、`Session.OnResponse` | 按 RpcId 唤醒等待中的 `Call` |

## 源码定位

- 客户端串联：`Packages/cn.etetet.login/Scripts/Hotfix/Client/NetClient/Main2NetClient_LoginHandler.cs`
- 客户端建路由 Session：`Packages/cn.etetet.login/Scripts/Hotfix/Client/NetClient/Router/RouterHelper.cs`
- Realm 入站与业务：`Packages/cn.etetet.login/Scripts/Hotfix/Server/Realm/NetComponentOnReadInvoker_Realm.cs`、`C2R_LoginHandler.cs`
- Gate 取 key 与登录：`Packages/cn.etetet.login/Scripts/Hotfix/Server/Gate/R2G_GetLoginKeyHandler.cs`、`C2G_LoginGateHandler.cs`
- 消息发送与 RPC 回调：`Packages/cn.etetet.core/Scripts/Hotfix/Share/Message/SessionSystem.cs`
- 会话消息分发：`Packages/cn.etetet.core/Scripts/Model/Share/Message/MessageSessionDispatcher.cs`
- Gate/Realm 网络初始化：`Packages/cn.etetet.login/Scripts/Hotfix/Server/Realm/FiberInit_Realm.cs`、`FiberInit_Gate.cs`

