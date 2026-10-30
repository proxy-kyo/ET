using System.Collections.Generic;

namespace ET.Server
{
    [Event(SceneType.Legend)]
    public class LegendEntryEvent2_InitServer: AEvent<Scene, LegendEntryEvent2>
    {
        protected override async ETTask Run(Scene root, LegendEntryEvent2 args)
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

            HashSet<int> gateZones = new();
            if (startProcessConfig != null)
            {
                var scenes = fiber.GetSingleton<StartSceneConfigCategory>().GetByProcess(process);
                foreach (StartSceneConfig startConfig in scenes)
                {
                    int sceneType = SceneTypeSingleton.Instance.GetSceneType(startConfig.SceneType);
                    if (sceneType == SceneType.Gate)
                    {
                        gateZones.Add(startConfig.Zone);
                    }

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

            // Gate 按 zone 查找地图，只在本进程存在 Gate 的 zone 创建地图纤程。
            foreach (int zone in gateZones)
            {
                await fiber.CreateFiber(SchedulerType.ThreadPool, zone, IdGenerater.Instance.GenerateId(), SceneType.Map,
                    $"{LegendMapName.First}@{process}@{Options.Instance.ReplicaIndex}");
            }

            root = rootRef;
            if (Options.Instance.Console == 1)
            {
                root.AddComponent<ConsoleComponent>();
            }
        }
    }
}
