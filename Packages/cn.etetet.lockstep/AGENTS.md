# cn.etetet.lockstep

- 遵守 `../cn.etetet.harness/AGENTS.md`。
- login 已带 Gate 读消息入口；lockstep 与 login 同时编译时，两份 `[Invoke(SceneType.Gate)]` 会冲突，需要合并。
