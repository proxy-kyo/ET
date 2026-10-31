using ET.Server;
using Unity.Mathematics;

namespace ET.Test
{
    public class Legend_MovementIntegration_Test: ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Legend_MovementIntegration_Test));
            Scene root = scope.TestFiber.Root;
            root.AddComponent<TimerComponent>();
            root.AddComponent<MessageSender>();
            Scene scene = EntitySceneFactory.CreateScene(root, IdGenerater.Instance.GenerateId(), SceneType.Map,
                nameof(Legend_MovementIntegration_Test));
            scene.AddComponent<GridMapComponent, int, int, float>(64, 64, LegendMapConst.CellSize);
            GridOccupancyComponent occupancy = scene.AddComponent<GridOccupancyComponent>();
            scene.AddComponent<AOIManagerComponent>();
            UnitComponent units = scene.AddComponent<UnitComponent>();
            Unit first = Enter(units);
            Unit second = Enter(units);
            GridMoveComponent firstMove = first.GetComponent<GridMoveComponent>();
            GridMoveComponent secondMove = second.GetComponent<GridMoveComponent>();
            AOIEntity firstAOI = first.GetComponent<AOIEntity>();
            AOIEntity secondAOI = second.GetComponent<AOIEntity>();
            if (!firstMove.Cell.Equals(new int2(32, 32)) ||
                firstMove.Cell.Equals(secondMove.Cell) || occupancy.Occupants.Count != 2)
            {
                return 1;
            }
            if (!firstAOI.SamePhase(secondAOI) || !firstAOI.GetSeeUnits().ContainsKey(first.Id) ||
                !firstAOI.GetSeeUnits().ContainsKey(second.Id) || !secondAOI.GetSeeUnits().ContainsKey(first.Id))
            {
                return 2;
            }
            GridMoveHelper.Enter(first, new int2(49, 32));
            if (GridMoveHelper.MoveTo(first, new int2(50, 32)) != ErrorCode.ERR_Success ||
                !firstMove.Cell.Equals(new int2(50, 32)) || firstAOI.Cell.Id != AOIHelper.CreateCellId(1, 0))
            {
                return 3;
            }
            long firstId = first.Id;
            first.RemoveComponent<AOIEntity>();
            units.Remove(firstId);
            if (occupancy.Occupants.Count != 1 || secondAOI.GetSeeUnits().ContainsKey(firstId))
            {
                return 4;
            }
            second.RemoveComponent<AOIEntity>();
            units.Remove(second.Id);
            return occupancy.Occupants.Count == 0 ? ErrorCode.ERR_Success : 5;
        }

        private static Unit Enter(UnitComponent units)
        {
            Unit unit = units.AddChild<Unit, int>(LegendUnitConfigId.Player);
            unit.UnitType = UnitType.Player;
            units.Add(unit);
            NumericComponent numeric = unit.AddComponent<NumericComponent>();
            numeric.SetNoEvent(NumericType.AOI, LegendMapConst.AOIRange);
            numeric.SetNoEvent(NumericType.Phase, (long)PhaseType.Normal);
            unit.AddComponent<GridMoveComponent>();
            GridMoveHelper.Enter(unit, new int2(32, 32));
            unit.AddComponent<AOIEntity>();
            return unit;
        }
    }
}
