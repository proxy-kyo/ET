using Unity.Mathematics;

namespace ET.Client
{
    public static class LegendAttackHelper
    {
        [EnableGetComponent(typeof(TimerComponent))]
        public static async ETTask<int> Attack(Scene root, long targetId)
        {
            EntityRef<Scene> rootRef = root;
            long actionId = ++root.GetComponent<LegendClientActionComponent>().ActionId;
            long deadline = root.GetSingleton<TimeInfo>().ClientNow() + LegendCombatConst.StuckTimeoutMs;
            int2 lastSelfCell = default;
            int2 lastSentCell = default;
            bool hasLastSentCell = false;
            while (true)
            {
                if (actionId != root.GetComponent<LegendClientActionComponent>().ActionId)
                {
                    return ErrorCode.ERR_LegendAttackCancelled;
                }
                Scene scene = root.CurrentScene();
                GridMapComponent map = scene.GetComponent<GridMapComponent>();
                UnitComponent units = scene.GetComponent<UnitComponent>();
                Unit self = units.Get(root.GetComponent<PlayerComponent>().MyId);
                Unit target = units.Get(targetId);
                if (self == null || target == null)
                {
                    return ErrorCode.ERR_LegendAttackTargetNotFound;
                }
                int2 selfCell = GridHelper.WorldToCell(map, self.Position);
                int2 targetCell = GridHelper.WorldToCell(map, target.Position);
                if (!selfCell.Equals(lastSelfCell))
                {
                    lastSelfCell = selfCell;
                    deadline = root.GetSingleton<TimeInfo>().ClientNow() + LegendCombatConst.StuckTimeoutMs;
                }
                int2 delta = targetCell - selfCell;
                if (math.max(math.abs(delta.x), math.abs(delta.y)) == 1)
                {
                    C2M_LegendAttack request = C2M_LegendAttack.Create();
                    request.TargetUnitId = targetId;
                    using M2C_LegendAttack response = (M2C_LegendAttack)await root.GetComponent<ClientSenderComponent>()
                        .Call(request, needException: false);
                    root = rootRef;
                    if (root == null || actionId != root.GetComponent<LegendClientActionComponent>().ActionId)
                    {
                        return ErrorCode.ERR_LegendAttackCancelled;
                    }
                    return response.Error;
                }
                if (TryFindApproachCell(map, units, selfCell, targetCell, out int2 cell) &&
                    (!hasLastSentCell || !lastSentCell.Equals(cell)))
                {
                    LegendMoveHelper.Send(root, cell);
                    lastSentCell = cell;
                    hasLastSentCell = true;
                }
                if (root.GetSingleton<TimeInfo>().ClientNow() >= deadline)
                {
                    return ErrorCode.ERR_LegendAttackNotAdjacent;
                }
                await root.GetComponent<TimerComponent>().WaitAsync(100);
                root = rootRef;
                if (root == null)
                {
                    return ErrorCode.ERR_LegendAttackCancelled;
                }
            }
        }

        private static bool TryFindApproachCell(GridMapComponent map, UnitComponent units,
            int2 selfCell, int2 targetCell, out int2 best)
        {
            best = default;
            int bestDistance = int.MaxValue;
            for (int dy = -1; dy <= 1; ++dy)
            {
                for (int dx = -1; dx <= 1; ++dx)
                {
                    if (dx == 0 && dy == 0) continue;
                    int2 cell = targetCell + new int2(dx, dy);
                    if (GridHelper.IsBlocked(map, cell)) continue;
                    bool occupied = false;
                    foreach (Unit visible in units.Children.Values)
                    {
                        if (GridHelper.WorldToCell(map, visible.Position).Equals(cell))
                        {
                            occupied = true;
                            break;
                        }
                    }
                    if (occupied) continue;
                    int2 toCell = cell - selfCell;
                    int distance = toCell.x * toCell.x + toCell.y * toCell.y;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = cell;
                    }
                }
            }
            return bestDistance != int.MaxValue;
        }
    }
}
