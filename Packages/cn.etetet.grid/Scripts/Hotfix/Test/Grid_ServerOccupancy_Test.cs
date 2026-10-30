using System;
using ET.Server;
using Unity.Mathematics;

namespace ET.Test
{
    public class Grid_ServerOccupancy_Test: ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Grid_ServerOccupancy_Test));
            Scene scene = scope.TestFiber.Root;
            scene.AddComponent<TimerComponent>();
            GridMapComponent map = scene.AddComponent<GridMapComponent, int, int, float>(8, 8, 2f);
            GridOccupancyComponent occupancy = scene.AddComponent<GridOccupancyComponent>();
            UnitComponent units = scene.AddComponent<UnitComponent>();
            Unit first = units.AddChild<Unit, int>(1);
            GridMoveComponent move = first.AddComponent<GridMoveComponent>();
            Unit second = units.AddChild<Unit, int>(1);
            second.AddComponent<GridMoveComponent>();
            int2 want = new(3, 3);
            int2 firstCell = GridMoveHelper.Enter(first, want);
            int2 secondCell = GridMoveHelper.Enter(second, want);
            if (!firstCell.Equals(want) || firstCell.Equals(secondCell) ||
                math.csum(math.abs(secondCell - want)) != 1 || occupancy.Occupants.Count != 2 ||
                occupancy.Occupants[GridHelper.Key(firstCell)] != first.Id ||
                occupancy.Occupants[GridHelper.Key(secondCell)] != second.Id ||
                !first.Position.Equals(GridHelper.CellToWorld(map, firstCell)))
            {
                return 1;
            }
            first.Dispose();
            if (occupancy.Occupants.ContainsKey(GridHelper.Key(firstCell)) || occupancy.Occupants.Count != 1 ||
                occupancy.Occupants[GridHelper.Key(secondCell)] != second.Id)
            {
                return 2;
            }

            first = units.AddChild<Unit, int>(1);
            move = first.AddComponent<GridMoveComponent>();
            GridMoveHelper.Enter(first, new int2(0, 0));
            if (move.StepMs != GridConst.DefaultStepMs ||
                GridMoveHelper.MoveTo(first, new int2(2, 0)) != ErrorCode.ERR_Success ||
                !move.Cell.Equals(new int2(1, 0)) || move.Index != 1 || move.TimerId == 0 ||
                occupancy.Occupants.ContainsKey(GridHelper.Key(new int2(0, 0))) ||
                occupancy.Occupants[GridHelper.Key(move.Cell)] != first.Id ||
                math.distance(first.Forward, new float3(1, 0, 0)) > 0.001f)
            {
                return 3;
            }
            long timerId = move.TimerId;
            if (GridMoveHelper.MoveTo(first, new int2(2, 0)) != ErrorCode.ERR_Success || move.TimerId != timerId || move.Index != 1)
            {
                return 4;
            }
            map.Blocked[2] = true;
            if (GridMoveHelper.MoveTo(first, new int2(2, 0)) != ErrorCode.ERR_GridTargetInvalid ||
                GridMoveHelper.MoveTo(first, new int2(-1, 0)) != ErrorCode.ERR_GridTargetInvalid)
            {
                return 5;
            }
            map.Blocked[2] = false;
            GridMoveHelper.Stop(first);
            if (move.TimerId != 0 || move.Path.Count != 0 || !move.Cell.Equals(new int2(1, 0)))
            {
                return 6;
            }
            if (GridMoveHelper.MoveTo(first, new int2(2, 1)) != ErrorCode.ERR_Success || !move.Cell.Equals(new int2(2, 1)) ||
                !first.Position.Equals(new float3(5, 0, 3)) ||
                math.distance(first.Forward, math.normalize(new float3(1, 0, 1))) > 0.001f)
            {
                return 7;
            }
            int2 lastCell = move.Cell;
            first.Dispose();
            if (occupancy.Occupants.ContainsKey(GridHelper.Key(lastCell)))
            {
                return 8;
            }

            first = units.AddChild<Unit, int>(1);
            move = first.AddComponent<GridMoveComponent>();
            GridMoveHelper.Enter(first, new int2(0, 0));
            GridMoveHelper.MoveTo(first, new int2(3, 0));
            GridMoveHelper.Enter(second, new int2(2, 0));
            Tick(first);
            if (move.BlockedWaits != 1 || move.TimerId == 0 || !move.Cell.Equals(new int2(1, 0)))
            {
                return 9;
            }
            Tick(first);
            if (move.BlockedWaits != 0 || move.TimerId == 0 || move.Cell.Equals(new int2(1, 0)) ||
                occupancy.Occupants[GridHelper.Key(new int2(2, 0))] != second.Id)
            {
                return 10;
            }
            GridMoveHelper.Stop(first);
            GridMoveHelper.Enter(first, new int2(0, 0));
            GridMoveHelper.MoveTo(first, new int2(2, 0));
            Tick(first);
            Tick(first);
            if (move.TimerId != 0 || move.Path.Count != 0 || !move.Cell.Equals(new int2(1, 0)))
            {
                return 11;
            }
            GridMoveHelper.MoveTo(first, new int2(1, 1));
            Tick(first);
            if (move.TimerId != 0 || move.Path.Count != 0 || !move.Cell.Equals(new int2(1, 1)))
            {
                return 12;
            }

            map.Blocked = new bool[map.Width * map.Height];
            for (int i = 0; i < map.Blocked.Length; ++i)
            {
                map.Blocked[i] = true;
            }
            try
            {
                GridMoveHelper.Enter(second, want);
                return 13;
            }
            catch (Exception)
            {
                return ErrorCode.ERR_Success;
            }
        }

        private static void Tick(Unit unit)
        {
            GridMoveComponent move = unit.GetComponent<GridMoveComponent>();
            unit.Root().TimerComponent.Remove(ref move.TimerId);
            GridMoveHelper.Step(unit);
        }
    }
}
