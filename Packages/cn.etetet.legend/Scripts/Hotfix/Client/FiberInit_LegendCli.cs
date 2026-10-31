namespace ET.Client
{
    [Invoke(SceneType.LegendCli)]
    public class FiberInit_LegendCli: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<TimerComponent>();
            root.AddComponent<ProcessInnerSender>();
            EntityRef<Scene> rootRef = root;
            LegendCliComponent cli = root.AddComponent<LegendCliComponent>();
            await cli.CreateClient(false, true);
            root = rootRef;
            cli = root.GetComponent<LegendCliComponent>();
            cli.CurrentIndex = 1;
            cli.StartReading();
        }
    }
}
