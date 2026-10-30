# cn.etetet.grid

## 规则

- 遵守 `Packages/cn.etetet.harness/AGENTS.md`；一类一文件，Entity 只放数据，逻辑放 System/Helper。
- 服务端权威，8 方向单步，一格一人；静态地形与服务端占用分离。
- 坐标统一使用 `int2`（x 列、y 行），世界坐标为 XZ 平面上的格子中心。
- 玩家与原住居民统一调用 `GridMoveHelper`，不得按 UnitType 分支；有无客户端只看 UnitGateInfoComponent。
- 不引用 move/recast 类型，不承载 legend 处理器或客户端表现。
- 不使用静态可变集合；await 后通过 EntityRef 重新获取实体。
- `.meta`、`AssemblyReference.asmref` 和工程文件由 Unity/ET 工具生成，不手写。

## 公共入口

- `GridHelper`：坐标转换、地形校验、禁止切角的 A*（占用目标允许寻路）。
- `GridMoveHelper.Enter / MoveTo / Stop`：占位、移动、停止；调用前地图已有 GridMapComponent、GridOccupancyComponent，Unit 已有 GridMoveComponent。
- `MapMessageHelper.NoticeClient(unit, message, NoticeType.Broadcast)`：复用 map 广播，仅发送给有 Gate 的观察单位，无 AOI 时直接返回。
- `Proto/Grid_C_11600.proto`：仅定义消息，不包含消息处理器。

## 最小验证

1. 导出协议：`dotnet ./Bin/ET.Proto2CS.dll`；恢复其他包生成文件，保留新增 Grid 文件。
2. UnityBridge 依次执行 AssetRefreshRequest、ET/Init/CodeMode、RegenProject、Compile，并等待各项成功。
3. 唯一编译命令：`dotnet build ET.sln`。
4. `"Test --Name=Grid.*" | dotnet ./Bin/ET.App.dll --SceneName=Test`，六个用例全部通过。
