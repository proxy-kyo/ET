using ET.Server;

namespace ET.Test
{
    [Event(SceneType.All)]
    public class FiberDestroyEvent_TestFiberDatabaseCleanup : AEvent<Scene, FiberDestroyEvent>
    {
        protected override async ETTask Run(Scene scene, FiberDestroyEvent args)
        {
            TestFiberDatabaseCleanupComponent cleanupComponent = scene.GetComponent<TestFiberDatabaseCleanupComponent>();
            if (cleanupComponent == null)
            {
                return;
            }

            await cleanupComponent.CleanupAsync(nameof(FiberDestroyEvent));
        }
    }
}
