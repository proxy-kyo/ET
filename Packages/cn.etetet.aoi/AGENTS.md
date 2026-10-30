# cn.etetet.aoi

- 遵守 `Packages/cn.etetet.harness/AGENTS.md`；Entity 只放数据，一类一文件。
- UnitGateInfoComponent 位于本包；AOIEntity.Awake 根据该组件是否存在快照 HasClient，不按 UnitType 判断客户端连接。
- SeePlayers / BeSeePlayers 仅记录有客户端的单位；无客户端的单位之间只建立 SeeUnits / BeSeeUnits，服务端 AI 仍可查询视野。
- 连接状态变化由调用方先删除再添加 AOIEntity，重新计算 HasClient 和订阅。
- NumericComponent（AOI、Phase）及可选的 UnitGateInfoComponent 须在添加 AOIEntity 前准备好。
