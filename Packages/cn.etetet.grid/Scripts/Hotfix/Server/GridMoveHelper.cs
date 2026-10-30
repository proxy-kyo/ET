using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    public static class GridMoveHelper
    {
        public static int2 Enter(Unit unit, int2 want)
        {
            GridMapComponent map = unit.Scene().GetComponent<GridMapComponent>();
            GridOccupancyComponent occupancy = unit.Scene().GetComponent<GridOccupancyComponent>();
            if (!GridHelper.IsBlocked(map, want) && !IsOccupied(occupancy.Occupants, unit.Id, want))
            {
                return Occupy(unit, map, occupancy, want);
            }

            for (int radius = 1; radius <= GridConst.MaxEnterRadius; ++radius)
            {
                int bestDistance = int.MaxValue;
                int2 best = default;
                for (int dy = -radius; dy <= radius; ++dy)
                {
                    for (int dx = -radius; dx <= radius; ++dx)
                    {
                        if (math.max(math.abs(dx), math.abs(dy)) != radius)
                        {
                            continue;
                        }
                        int2 cell = want + new int2(dx, dy);
                        int distance = dx * dx + dy * dy;
                        if (distance < bestDistance && !GridHelper.IsBlocked(map, cell) && !IsOccupied(occupancy.Occupants, unit.Id, cell))
                        {
                            best = cell;
                            bestDistance = distance;
                        }
                    }
                }
                if (bestDistance != int.MaxValue)
                {
                    return Occupy(unit, map, occupancy, best);
                }
            }
            throw new Exception($"No free grid cell within radius {GridConst.MaxEnterRadius} of {want}");
        }

        public static int MoveTo(Unit unit, int2 target)
        {
            GridMapComponent map = unit.Scene().GetComponent<GridMapComponent>();
            if (GridHelper.IsBlocked(map, target))
            {
                return ErrorCode.ERR_GridTargetInvalid;
            }
            GridMoveComponent self = unit.GetComponent<GridMoveComponent>();
            if (self.Target.Equals(target) && self.TimerId != 0)
            {
                return ErrorCode.ERR_Success;
            }

            Dictionary<long, long> occupants = unit.Scene().GetComponent<GridOccupancyComponent>().Occupants;
            long unitId = unit.Id;
            List<int2> path = new();
            if (!GridHelper.FindPath(map, self.Cell, target, cell => IsOccupied(occupants, unitId, cell), path))
            {
                return ErrorCode.ERR_GridPathNotFound;
            }
            self.Path.Clear();
            self.Path.AddRange(path);
            self.Index = 0;
            self.Target = target;
            self.BlockedWaits = 0;
            if (self.TimerId == 0)
            {
                Step(unit);
            }
            return ErrorCode.ERR_Success;
        }

        public static void Stop(Unit unit)
        {
            GridMoveComponent self = unit.GetComponent<GridMoveComponent>();
            self.Path.Clear();
            self.Index = 0;
            self.BlockedWaits = 0;
            unit.Root().TimerComponent.Remove(ref self.TimerId);
            M2C_GridStop message = M2C_GridStop.Create();
            message.UnitId = unit.Id;
            message.X = self.Cell.x;
            message.Y = self.Cell.y;
            GridMessageHelper.Broadcast(unit, message);
        }

        internal static void Step(Unit unit)
        {
            GridMoveComponent self = unit.GetComponent<GridMoveComponent>();
            if (self.Index >= self.Path.Count)
            {
                self.Path.Clear();
                self.Index = 0;
                self.TimerId = 0;
                return;
            }

            GridMapComponent map = unit.Scene().GetComponent<GridMapComponent>();
            GridOccupancyComponent occupancy = unit.Scene().GetComponent<GridOccupancyComponent>();
            Dictionary<long, long> occupants = occupancy.Occupants;
            long unitId = unit.Id;
            int2 next = self.Path[self.Index];
            if (!GridHelper.CanStep(map, self.Cell, next))
            {
                Stop(unit);
                return;
            }
            if (IsOccupied(occupants, unitId, next))
            {
                ++self.BlockedWaits;
                if (self.BlockedWaits == 1)
                {
                    Schedule(self, self.StepMs);
                    return;
                }
                if (self.BlockedWaits == 2 && GridHelper.FindPath(map, self.Cell, self.Target,
                        cell => IsOccupied(occupants, unitId, cell), self.Path))
                {
                    self.Index = 0;
                    Step(unit);
                    return;
                }
                Stop(unit);
                return;
            }

            int2 delta = next - self.Cell;
            Occupy(unit, map, occupancy, next);
            unit.Forward = math.normalize(new float3(delta.x, 0, delta.y));
            ++self.Index;
            self.BlockedWaits = 0;
            int duration = delta.x != 0 && delta.y != 0 ? self.StepMs * 14 / 10 : self.StepMs;
            M2C_GridStep message = M2C_GridStep.Create();
            message.UnitId = unit.Id;
            message.X = next.x;
            message.Y = next.y;
            message.DurationMs = duration;
            GridMessageHelper.Broadcast(unit, message);
            Schedule(self, duration);
        }

        private static int2 Occupy(Unit unit, GridMapComponent map, GridOccupancyComponent occupancy, int2 cell)
        {
            GridMoveComponent self = unit.GetComponent<GridMoveComponent>();
            occupancy.Occupants[GridHelper.Key(cell)] = unit.Id;
            long oldKey = GridHelper.Key(self.Cell);
            if (!self.Cell.Equals(cell) && occupancy.Occupants.TryGetValue(oldKey, out long owner) && owner == unit.Id)
            {
                occupancy.Occupants.Remove(oldKey);
            }
            self.Cell = cell;
            unit.Position = GridHelper.CellToWorld(map, cell);
            return cell;
        }

        private static bool IsOccupied(Dictionary<long, long> occupants, long unitId, int2 cell)
        {
            return occupants.TryGetValue(GridHelper.Key(cell), out long owner) && owner != unitId;
        }

        private static void Schedule(GridMoveComponent self, int duration)
        {
            self.TimerId = self.Root().TimerComponent.NewOnceTimer(
                self.GetSingleton<TimeInfo>().ServerNow() + duration, TimerInvokeType.GridStep, self);
        }
    }
}
