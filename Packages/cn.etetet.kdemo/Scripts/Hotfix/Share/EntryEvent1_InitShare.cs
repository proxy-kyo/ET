namespace ET
{
    [Event(SceneType.All)]
    public class KDemoEntryEvent1_InitShare: AEvent<Scene, KDemoEntryEvent1>
    {
        protected override async ETTask Run(Scene root, KDemoEntryEvent1 args)
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
