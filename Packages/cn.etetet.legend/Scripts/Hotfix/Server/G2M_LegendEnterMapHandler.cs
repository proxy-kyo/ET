using Unity.Mathematics;

namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class G2M_LegendEnterMapHandler: MessageHandler<Scene, G2M_LegendEnterMap, M2G_LegendEnterMap>
    {
        protected override async ETTask Run(Scene scene, G2M_LegendEnterMap request, M2G_LegendEnterMap response)
        {
            EntityRef<Scene> sceneRef = scene;
            using var _ = await scene.CoroutineLockComponent.Wait(CoroutineLockType.LegendEnterMap, request.PlayerId);
            scene = sceneRef;
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            EntityRef<UnitComponent> unitComponentRef = unitComponent;
            Unit oldUnit = unitComponent.Get(request.PlayerId);
            if (oldUnit != null)
            {
                // ponytail: 断线清理暂靠重登覆盖，login 断线不会通知 Map，后续做断线下线时补 G2M 通知。
                oldUnit.RemoveComponent<AOIEntity>();
                await oldUnit.RemoveLocation(LocationType.Unit);
                unitComponent = unitComponentRef;
                unitComponent.Remove(request.PlayerId);
            }

            Unit unit = unitComponent.AddChildWithId<Unit, int>(request.PlayerId, LegendUnitConfigId.Player);
            unit.UnitType = unit.Config().UnitType;
            unitComponent.Add(unit);
            unit.AddComponent<UnitGateInfoComponent>().ActorId = request.GateActorId;
            unit.AddComponent<MailBoxComponent, int>(MailBoxType.OrderedMessage);
            NumericComponent numeric = unit.AddComponent<NumericComponent>();
            numeric.SetNoEvent(NumericType.AOI, LegendMapConst.AOIRange);
            numeric.SetNoEvent(NumericType.Phase, (long)PhaseType.Normal);
            unit.AddComponent<GridMoveComponent>();
            GridMoveHelper.Enter(unit, new int2(LegendMapConst.SpawnX, LegendMapConst.SpawnY));
            unit.AddComponent<AOIEntity>();
            EntityRef<Unit> unitRef = unit;
            await unit.AddLocation(LocationType.Unit);
            unit = unitRef;
            response.UnitId = unit.Id;
            scene = sceneRef;
            response.MapSceneId = scene.Id;
        }
    }
}
