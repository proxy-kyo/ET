namespace ET.Server
{
    [Event(SceneType.KDemo)]
    public class KDemoEntryEvent2_InitServer: AEvent<Scene, KDemoEntryEvent2>
    {
        protected override async ETTask Run(Scene root, KDemoEntryEvent2 args)
        {
            // 服务端初始化占位。
            // statesync 原包在此根据 StartProcessConfig / StartSceneConfig 创建
            // NetInner、ServiceDiscoveryAgent 及各业务纤程；骨架包不拉入这些重依赖，
            // 需要时按 statesync 的 EntryEvent2_InitServer 补齐。
            await ETTask.CompletedTask;
        }
    }
}
