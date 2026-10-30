namespace ET
{
    [Event(SceneType.All)]
    public class LegendEntryEvent1_InitShare: AEvent<Scene, LegendEntryEvent1>
    {
        protected override async ETTask Run(Scene root, LegendEntryEvent1 args)
        {
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ObjectWait>();
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<ProcessInnerSender>();
            await ETTask.CompletedTask;
        }
    }
}
