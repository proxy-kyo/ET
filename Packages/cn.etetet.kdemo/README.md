# cn.etetet.kdemo

基于 `cn.etetet.statesync` 创建的代码骨架包。

- PackageId: 200
- SceneType: `SceneType.KDemo`（= 200 * 1000 + 1）

## 结构

复制了 statesync 的入口链结构（纤程初始化 + 三段 EntryEvent），
所有入口类型加 `KDemo` 前缀以避免与 statesync 在 ET 共享程序集中的类型重名。

- `Scripts/Model/Share/PackageType.cs` — `PackageType.KDemo = 200`
- `Scripts/Model/Share/SceneType.cs` — `SceneType.KDemo`
- `Scripts/Model/Share/EntryEvent.cs` — `KDemoEntryEvent1/2/3`
- `Scripts/Hotfix/Share/FiberInit_KDemo.cs` — `[Invoke(SceneType.KDemo)]` 入口
- `Scripts/Hotfix/Share/EntryEvent1_InitShare.cs` — Share 初始化
- `Scripts/Hotfix/Server/EntryEvent2_InitServer.cs` — 根据 StartConfig 创建本地服务端纤程
- `Scripts/HotfixView/Client/EntryEvent3_InitClient.cs` — 创建 KDEMO 客户端纤程
- `Scripts/HotfixView/Client/KDemoClientInitFinish_CreateLoginView.cs` — 初始化 YIUI 并打开 statesync 的登录面板

登录流程复用 statesync 的 YIUI `LoginPanelComponent` 与 `cn.etetet.login`，成功后关闭登录面板并记录 `PlayerId`；当前不包含进入地图。
未复制 statesync 的 YIUI 生成代码或 prefab。
