namespace ET.Client
{
    [Event(SceneType.KDemoClient)]
    public class KDemoLoginFinish_CloseLoginUI: AEvent<Scene, LoginFinish>
    {
        protected override async ETTask Run(Scene root, LoginFinish args)
        {
            long playerId = root.GetComponent<PlayerComponent>().MyId;
            await root.YIUIMgr().ClosePanelAsync<KLoginPanelComponent>();
            Log.Info($"KDEMO login succeeded. PlayerId: {playerId}");
        }
    }
}
