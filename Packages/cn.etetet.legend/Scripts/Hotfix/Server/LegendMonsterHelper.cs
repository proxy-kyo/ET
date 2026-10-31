using Unity.Mathematics;

namespace ET.Server
{
    public static class LegendMonsterHelper
    {
        public static Unit Spawn(Scene scene, int areaIndex)
        {
            LegendMonsterArea area = scene.GetComponent<LegendMapComponent>().MonsterAreas[areaIndex];
            GridMapComponent map = scene.GetComponent<GridMapComponent>();
            GridOccupancyComponent occupancy = scene.GetComponent<GridOccupancyComponent>();
            for (int attempt = 0; attempt < 50; ++attempt)
            {
                int2 cell = new(RandomGenerator.RandomNumber(area.Min.x, area.Max.x + 1),
                    RandomGenerator.RandomNumber(area.Min.y, area.Max.y + 1));
                if (GridHelper.IsBlocked(map, cell) || occupancy.Occupants.ContainsKey(GridHelper.Key(cell)))
                {
                    continue;
                }
                UnitComponent units = scene.GetComponent<UnitComponent>();
                Unit unit = units.AddChildWithId<Unit, int>(IdGenerater.Instance.GenerateId(), LegendUnitConfigId.Boar);
                unit.UnitType = unit.Config().UnitType;
                units.Add(unit);
                LegendUnitHelper.InitNumeric(unit);
                unit.AddComponent<LegendCombatComponent>();
                unit.AddComponent<LegendMonsterComponent>().AreaIndex = areaIndex;
                unit.AddComponent<GridMoveComponent>();
                GridMoveHelper.Enter(unit, cell);
                unit.AddComponent<AOIEntity>();
                return unit;
            }
            return null;
        }

        public static void Die(Unit unit)
        {
            int areaIndex = unit.GetComponent<LegendMonsterComponent>().AreaIndex;
            Scene scene = unit.Scene();
            long id = unit.Id;
            unit.RemoveComponent<AOIEntity>();
            scene.GetComponent<UnitComponent>().Remove(id);
            Respawn(scene, areaIndex).Coroutine();
        }

        [EnableGetComponent(typeof(TimerComponent))]
        private static async ETTask Respawn(Scene scene, int areaIndex)
        {
            EntityRef<Scene> sceneRef = scene;
            await scene.Root().GetComponent<TimerComponent>().WaitAsync(LegendCombatConst.RespawnMs);
            scene = sceneRef;
            if (scene == null)
            {
                return;
            }
            Spawn(scene, areaIndex);
        }
    }
}
