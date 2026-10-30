namespace ET.Client
{
    [MessageHandler(SceneType.Current)]
    public class M2C_LegendUnitsDisappearHandler: MessageHandler<Scene, M2C_LegendUnitsDisappear>
    {
        protected override async ETTask Run(Scene scene, M2C_LegendUnitsDisappear message)
        {
            UnitComponent units = scene.GetComponent<UnitComponent>();
            foreach (long id in message.UnitIds)
            {
                units.Remove(id);
            }
            await ETTask.CompletedTask;
        }
    }
}
