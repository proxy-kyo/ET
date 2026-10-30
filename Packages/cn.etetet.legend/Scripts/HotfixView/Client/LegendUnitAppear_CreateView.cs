namespace ET.Client
{
    [Event(SceneType.Current)]
    public class LegendUnitAppear_CreateView: AEvent<Scene, LegendUnitAppear>
    {
        protected override async ETTask Run(Scene scene, LegendUnitAppear args)
        {
            Unit unit = args.Unit;
            unit.AddComponent<LegendUnitViewComponent>();
            await ETTask.CompletedTask;
        }
    }
}
