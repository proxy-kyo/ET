namespace ET.Client
{
    [Event(SceneType.Legend)]
    public class LegendEntryEvent3_InitClient: AEvent<Scene, LegendEntryEvent3>
    {
        protected override async ETTask Run(Scene root, LegendEntryEvent3 args)
        {
            Fiber fiber = root.Fiber;
            await fiber.CreateFiber(SchedulerType.Parent, IdGenerater.Instance.GenerateId(), SceneType.LegendClient, "LegendClient");
        }
    }
}
