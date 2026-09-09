# cn.etetet.login

- 继承 `../cn.etetet.harness/AGENTS.md` 的项目规范。
- 登录凭据校验位于 Realm，必须在签发 Gate Key 之前完成。
- 当前按用户明确要求，仅允许固定账号 `kyo` 和密码 `111111`。
- 客户端收到 Realm 错误响应后必须停止登录，不得继续连接 Gate。
