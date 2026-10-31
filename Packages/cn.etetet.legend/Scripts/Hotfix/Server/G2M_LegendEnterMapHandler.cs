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
                await LegendUnitHelper.RemovePlayer(oldUnit);
                unitComponent = unitComponentRef;
            }

            Unit unit = unitComponent.AddChildWithId<Unit, int>(request.PlayerId, LegendUnitConfigId.Player);
            unit.UnitType = unit.Config().UnitType;
            unitComponent.Add(unit);
            unit.AddComponent<UnitGateInfoComponent>().ActorId = request.GateActorId;
            unit.AddComponent<MailBoxComponent, int>(MailBoxType.OrderedMessage);
            LegendUnitHelper.InitNumeric(unit);
            unit.AddComponent<LegendCombatComponent>();
            unit.AddComponent<GridMoveComponent>();
            scene = sceneRef;
            GridMoveHelper.Enter(unit, scene.GetComponent<LegendMapComponent>().Spawn);
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
