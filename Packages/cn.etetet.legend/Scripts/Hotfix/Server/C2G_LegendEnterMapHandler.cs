using System.Collections.Generic;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_LegendEnterMapHandler: MessageSessionHandler<C2G_LegendEnterMap, G2C_LegendEnterMap>
    {
        protected override async ETTask Run(Session session, C2G_LegendEnterMap request, G2C_LegendEnterMap response)
        {
            Player player = session.GetComponent<SessionPlayerComponent>().Player;
            EntityRef<Player> playerRef = player;
            Scene root = session.Root();
            EntityRef<Scene> rootRef = root;
            ServiceDiscoveryProxy serviceDiscoveryProxy = root.GetComponent<ServiceDiscoveryProxy>();
            EntityRef<ServiceDiscoveryProxy> serviceDiscoveryProxyRef = serviceDiscoveryProxy;
            List<ServiceInfo> maps = serviceDiscoveryProxy.GetBySceneTypeAndZone(SceneType.Map, root.Zone());
            if (maps.Count == 0)
            {
                await serviceDiscoveryProxy.SubscribeServiceChange("LegendMap",
                    new StringKV
                    {
                        { ServiceMetaKey.SceneType, SceneTypeSingleton.Instance.GetSceneName(SceneType.Map) },
                    });
                root = rootRef;
                serviceDiscoveryProxy = serviceDiscoveryProxyRef;
                maps = serviceDiscoveryProxy.GetBySceneTypeAndZone(SceneType.Map, root.Zone());
                if (maps.Count == 0)
                {
                    response.Error = ErrorCode.ERR_LegendMapNotFound;
                    return;
                }
            }

            ActorId mapActorId = maps[0].ActorId;
            player = playerRef;
            G2M_LegendEnterMap enterMap = G2M_LegendEnterMap.Create();
            enterMap.PlayerId = player.Id;
            enterMap.GateActorId = player.GetComponent<PlayerSessionComponent>().GetActorId();
            // 进图前记下地图：Call 还没返回就断线时，离开消息排在进图之后，Map 先进后删，不会留下幽灵 Unit。
            LegendPlayerMapComponent playerMap = player.GetComponent<LegendPlayerMapComponent>() ??
                    player.AddComponent<LegendPlayerMapComponent>();
            playerMap.MapActorId = mapActorId;
            using M2G_LegendEnterMap result = (M2G_LegendEnterMap)await root.GetComponent<MessageSender>().Call(mapActorId, enterMap, false);
            response.Error = result.Error;
            response.Message = result.Message;
            if (result.Error != ErrorCode.ERR_Success)
            {
                return;
            }

            response.MapSceneId = result.MapSceneId;
            response.MapName = LegendMapName.First;
            response.UnitId = result.UnitId;
        }
    }
}
