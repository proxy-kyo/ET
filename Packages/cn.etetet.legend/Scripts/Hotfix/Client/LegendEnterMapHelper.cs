namespace ET.Client
{
    public static class LegendEnterMapHelper
    {
        public static async ETTask EnterMapAsync(Scene root)
        {
            EntityRef<Scene> rootRef = root;
            using G2C_LegendEnterMap response = (G2C_LegendEnterMap)await root.GetComponent<ClientSenderComponent>()
                .Call(C2G_LegendEnterMap.Create());
            root = rootRef;
            CurrentSceneFactory.Create(response.MapSceneId, response.MapName, root.GetComponent<CurrentScenesComponent>());
            await EventSystem.Instance.PublishAsync(root, new LegendEnterMapFinish
            {
                MapName = response.MapName,
                UnitId = response.UnitId,
            });
        }
    }
}
