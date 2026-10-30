# cn.etetet.login

- 继承 `../cn.etetet.harness/AGENTS.md` 的项目规范。
- 登录凭据校验位于 Realm，必须在签发 Gate Key 之前完成。
- 账号 Trim 后转小写，限定 3–16 位 `[a-z0-9_]`；密码不 Trim，限定 6–32 位。
- 账号存 MongoDB，首次登录自动注册；PBKDF2 必须在线程池计算，禁止保存或记录明文密码。
- 新账号使用 InsertOneAsync；Name 和 CharacterIds 建唯一索引，Id 冲突重试最多三次。
- 账号和角色使用随机正 63 位 Id；角色统一调用 CharacterIdHelper.New，禁止编码来源信息。
- Gate 复用持久化角色 Id；旧会话销毁不能触发已被新会话接管的玩家登出。
- 客户端收到 Realm 错误响应后必须停止登录，不得继续连接 Gate。
