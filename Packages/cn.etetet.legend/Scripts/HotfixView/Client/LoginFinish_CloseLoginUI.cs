namespace ET.Client
{
    [Event(SceneType.LegendClient)]
    public class LoginFinish_CloseLoginUI: AEvent<Scene, LoginFinish>
    {
        protected override async ETTask Run(Scene root, LoginFinish args)
        {
            long playerId = root.GetComponent<PlayerComponent>().MyId;
            await root.YIUIMgr().ClosePanelAsync<LegendLoginPanelComponent>();
            Log.Info($"legend login succeeded. PlayerId: {playerId}");
        }
    }
}
