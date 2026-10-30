namespace ET.Client
{
    [Invoke(SceneType.LegendClient)]
    public class FiberInit_LegendClient: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<ObjectWait>();
            root.AddComponent<PlayerComponent>();
            root.AddComponent<CurrentScenesComponent>();
            await EventSystem.Instance.PublishAsync(root, new LegendClientInitFinish());
        }
    }
}
