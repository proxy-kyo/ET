using ET.Server;
using Unity.Mathematics;

namespace ET.Test
{
    public class Legend_Combat_Test: ATestHandler
    {
        [EnableGetComponent(typeof(NumericComponent))]
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Legend_Combat_Test));
            Scene root = scope.TestFiber.Root;
            root.AddComponent<TimerComponent>();
            root.AddComponent<MessageSender>();
            Scene scene = EntitySceneFactory.CreateScene(root, IdGenerater.Instance.GenerateId(), SceneType.Map,
                nameof(Legend_Combat_Test));
            LegendMapHelper.Load(scene, LegendMapName.First);
            GridOccupancyComponent occupancy = scene.AddComponent<GridOccupancyComponent>();
            scene.AddComponent<AOIManagerComponent>();
            UnitComponent units = scene.AddComponent<UnitComponent>();
            int areaIndex = scene.GetComponent<LegendMapComponent>().MonsterAreas.FindIndex(area => area.Name == "meadow_n");
            if (areaIndex < 0) return 1;
            Unit monster = LegendMonsterHelper.Spawn(scene, areaIndex);
            if (monster == null) return 2;
            long monsterId = monster.Id;
            int2 monsterCell = monster.GetComponent<GridMoveComponent>().Cell;
            NumericComponent monsterNumeric = monster.GetComponent<NumericComponent>();
            if (monsterNumeric.GetAsLong(NumericType.MaxHP) != 100 || monsterNumeric.GetAsLong(NumericType.HP) != 100)
            {
                return 3;
            }

            Unit player = units.AddChildWithId<Unit, int>(IdGenerater.Instance.GenerateId(), LegendUnitConfigId.Player);
            player.UnitType = player.Config().UnitType;
            units.Add(player);
            LegendUnitHelper.InitNumeric(player);
            LegendCombatComponent combat = player.AddComponent<LegendCombatComponent>();
            player.AddComponent<GridMoveComponent>();
            GridMoveHelper.Enter(player, scene.GetComponent<LegendMapComponent>().Spawn);
            player.AddComponent<AOIEntity>();
            if (LegendCombatHelper.Attack(player, monsterId) != ErrorCode.ERR_LegendAttackNotAdjacent) return 4;

            bool found = false;
            int2 adjacent = default;
            GridMapComponent map = scene.GetComponent<GridMapComponent>();
            for (int dy = -1; dy <= 1 && !found; ++dy)
            {
                for (int dx = -1; dx <= 1; ++dx)
                {
                    if (dx == 0 && dy == 0) continue;
                    int2 cell = monsterCell + new int2(dx, dy);
                    if (GridHelper.IsBlocked(map, cell) || occupancy.Occupants.ContainsKey(GridHelper.Key(cell))) continue;
                    adjacent = cell;
                    found = true;
                    break;
                }
            }
            if (!found) return 5;
            GridMoveHelper.Enter(player, adjacent);
            int2 delta = player.GetComponent<GridMoveComponent>().Cell - monsterCell;
            if (math.max(math.abs(delta.x), math.abs(delta.y)) != 1) return 6;
            if (LegendCombatHelper.Attack(player, monsterId) != ErrorCode.ERR_Success ||
                monsterNumeric.GetAsLong(NumericType.HP) != 75) return 7;
            if (LegendCombatHelper.Attack(player, monsterId) != ErrorCode.ERR_LegendAttackCooldown) return 8;
            for (int i = 0; i < 3; ++i)
            {
                combat.NextAttackTime = 0;
                if (LegendCombatHelper.Attack(player, monsterId) != ErrorCode.ERR_Success) return 9 + i;
            }
            if (units.Get(monsterId) != null || occupancy.Occupants.ContainsKey(GridHelper.Key(monsterCell))) return 12;
            if (LegendCombatHelper.Attack(player, player.Id) != ErrorCode.ERR_LegendAttackSelf) return 13;
            if (LegendCombatHelper.Attack(player, monsterId) != ErrorCode.ERR_LegendAttackTargetNotFound) return 14;
            return ErrorCode.ERR_Success;
        }
    }
}
