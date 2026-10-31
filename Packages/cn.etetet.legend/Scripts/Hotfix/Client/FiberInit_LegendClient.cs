namespace ET.Client
{
    [Invoke(SceneType.Client)]
    public class FiberInit_LegendClient: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.LegendClient);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<ObjectWait>();
            root.AddComponent<PlayerComponent>();
            root.AddComponent<LegendClientActionComponent>();
            root.AddComponent<CurrentScenesComponent>();
            root.AddComponent<LegendSceneMessagesComponent>();
            await EventSystem.Instance.PublishAsync(root, new LegendClientInitFinish());
        }
    }
}
