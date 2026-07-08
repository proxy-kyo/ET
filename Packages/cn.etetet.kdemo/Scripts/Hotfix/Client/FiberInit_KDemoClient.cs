namespace ET.Client
{
    // KDemo 自己的客户端纤程初始化，不复用 statesync 的 FiberInit_Client。
    // 只挂最小核心组件，不拉入 YIUI 登录流程与 statesync 的业务组件
    // (PlayerComponent / CurrentScenesComponent / QuestComponent / ItemComponent 等)。
    [Invoke(SceneType.KDemoClient)]
    public class FiberInit_KDemoClient: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;

            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<ObjectWait>();

            await ETTask.CompletedTask;
        }
    }
}
