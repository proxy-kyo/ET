using System;

namespace ET
{
    [Invoke(SceneType.Legend)]
    public class FiberInit_Legend: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Fiber fiber = fiberInit.Fiber;
            Scene root = fiber.Root;

            EntityRef<Scene> rootRef = root;
            await EventSystem.Instance.PublishAsync(root, new LegendEntryEvent1());
            root = rootRef;
            await EventSystem.Instance.PublishAsync(root, new LegendEntryEvent2());
            root = rootRef;
            await EventSystem.Instance.PublishAsync(root, new LegendEntryEvent3());
        }
    }
}
