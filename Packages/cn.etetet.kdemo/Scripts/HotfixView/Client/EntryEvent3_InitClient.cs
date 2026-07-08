namespace ET.Client
{
    [Event(SceneType.KDemo)]
    public class KDemoEntryEvent3_InitClient: AEvent<Scene, KDemoEntryEvent3>
    {
        protected override async ETTask Run(Scene root, KDemoEntryEvent3 args)
        {
            Fiber fiber = root.Fiber;
            await fiber.CreateFiber(SchedulerType.Parent, IdGenerater.Instance.GenerateId(), SceneType.KDemoClient, "KDemoClient");
        }
    }
}
