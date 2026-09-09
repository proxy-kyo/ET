namespace ET.Server
{
    [Event(SceneType.KDemo)]
    public class KDemoEntryEvent2_InitServer: AEvent<Scene, KDemoEntryEvent2>
    {
        protected override async ETTask Run(Scene root, KDemoEntryEvent2 args)
        {
            LogMsg.Instance.AddIgnore(typeof(ServiceHeartbeatRequest));
            LogMsg.Instance.AddIgnore(typeof(ServiceHeartbeatResponse));

            Fiber fiber = root.Fiber;
            EntityRef<Scene> rootRef = root;
            int process = Options.Instance.Process;
            StartProcessConfig startProcessConfig = fiber.GetSingleton<StartProcessConfigCategory>().Get(process);

            World.Instance.AddSingleton<AddressSingleton>();
            AddressHelper.SetInnerIPInnerPortOuterIP(fiber, startProcessConfig);

            await fiber.CreateFiber(SchedulerType.ThreadPool, 0, IdGenerater.Instance.GenerateId(), SceneType.NetInner,
                $"NetInner@{process}@{Options.Instance.ReplicaIndex}");
            await fiber.CreateFiber(SchedulerType.ThreadPool, 0, IdGenerater.Instance.GenerateId(), SceneType.ServiceDiscoveryAgent,
                $"ServiceDiscoveryAgent@{process}@{Options.Instance.ReplicaIndex}");

            if (startProcessConfig != null)
            {
                var scenes = fiber.GetSingleton<StartSceneConfigCategory>().GetByProcess(process);
                foreach (StartSceneConfig startConfig in scenes)
                {
                    int sceneType = SceneTypeSingleton.Instance.GetSceneType(startConfig.SceneType);
                    if (sceneType == SceneType.ServiceDiscovery)
                    {
                        await fiber.CreateFiber(SchedulerType.ThreadPool, 0, startConfig.Id, sceneType,
                            $"{startConfig.Name}@{process}@{Options.Instance.ReplicaIndex}");
                    }
                    else
                    {
                        await fiber.CreateFiber(SchedulerType.ThreadPool, startConfig.Zone, startConfig.Id, sceneType,
                            $"{startConfig.Name}@{process}@{Options.Instance.ReplicaIndex}");
                    }
                }
            }

            root = rootRef;
            if (Options.Instance.Console == 1)
            {
                root.AddComponent<ConsoleComponent>();
            }
        }
    }
}
