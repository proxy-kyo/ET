namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class G2M_LegendLeaveMapHandler: MessageHandler<Scene, G2M_LegendLeaveMap>
    {
        protected override async ETTask Run(Scene scene, G2M_LegendLeaveMap message)
        {
            EntityRef<Scene> sceneRef = scene;
            using var _ = await scene.CoroutineLockComponent.Wait(CoroutineLockType.LegendEnterMap, message.PlayerId);
            scene = sceneRef;
            Unit unit = scene.GetComponent<UnitComponent>().Get(message.PlayerId);
            if (unit == null)
            {
                return;
            }

            await LegendUnitHelper.RemovePlayer(unit);
        }
    }
}
