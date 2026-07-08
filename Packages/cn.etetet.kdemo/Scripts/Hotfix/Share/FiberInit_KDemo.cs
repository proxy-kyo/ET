using System;

namespace ET
{
    [Invoke(SceneType.KDemo)]
    public class FiberInit_KDemo: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Fiber fiber = fiberInit.Fiber;
            Scene root = fiber.Root;

            EntityRef<Scene> rootRef = root;
            await EventSystem.Instance.PublishAsync(root, new KDemoEntryEvent1());
            root = rootRef;
            await EventSystem.Instance.PublishAsync(root, new KDemoEntryEvent2());
            root = rootRef;
            await EventSystem.Instance.PublishAsync(root, new KDemoEntryEvent3());
        }
    }
}
