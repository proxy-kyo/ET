namespace ET.Client
{
    public static class LegendEnterMapHelper
    {
        public static async ETTask EnterMapAsync(Scene root)
        {
            EntityRef<Scene> rootRef = root;
            root.GetComponent<LegendSceneMessagesComponent>().Ready = false;
            using G2C_LegendEnterMap response = (G2C_LegendEnterMap)await root.GetComponent<ClientSenderComponent>()
                .Call(C2G_LegendEnterMap.Create());
            root = rootRef;
            root.CurrentScene()?.Dispose();
            Scene currentScene = CurrentSceneFactory.Create(response.MapSceneId, response.MapName, root.GetComponent<CurrentScenesComponent>());
            currentScene.AddComponent<UnitComponent>();
            currentScene.AddComponent<GridMapComponent, int, int, float>(LegendMapConst.Width, LegendMapConst.Height, LegendMapConst.CellSize);
            await EventSystem.Instance.PublishAsync(root, new LegendEnterMapFinish
            {
                MapName = response.MapName,
                UnitId = response.UnitId,
            });
            root = rootRef;
            LegendSceneMessagesComponent pending = root.GetComponent<LegendSceneMessagesComponent>();
            pending.Ready = true;
            MailBoxComponent mailbox = root.CurrentScene().GetComponent<MailBoxComponent>();
            while (pending.Messages.Count > 0)
            {
                var entry = pending.Messages.Dequeue();
                mailbox.Add(entry.FromFiber, entry.Message);
            }
        }
    }
}
