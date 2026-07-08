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
- `Scripts/Hotfix/Server/EntryEvent2_InitServer.cs` — Server 初始化占位
- `Scripts/HotfixView/Client/EntryEvent3_InitClient.cs` — Client 初始化占位

未复制 statesync 的美术资源、整套 YIUI 界面、Luban 配置、DotNet 工程。
