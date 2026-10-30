using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    public static class GridHelper
    {
        public static long Key(int2 cell) => ((long)cell.x << 32) | (uint)cell.y;

        public static int2 WorldToCell(GridMapComponent map, float3 pos)
        {
            return (int2)math.floor(new float2(pos.x, pos.z) / map.CellSize);
        }

        public static float3 CellToWorld(GridMapComponent map, int2 cell)
        {
            return new float3((cell.x + 0.5f) * map.CellSize, 0, (cell.y + 0.5f) * map.CellSize);
        }

        public static bool InBounds(GridMapComponent map, int2 cell)
        {
            return cell.x >= 0 && cell.x < map.Width && cell.y >= 0 && cell.y < map.Height;
        }

        public static bool IsBlocked(GridMapComponent map, int2 cell)
        {
            return !InBounds(map, cell) || map.Blocked[cell.y * map.Width + cell.x];
        }

        public static bool CanStep(GridMapComponent map, int2 from, int2 to)
        {
            int2 delta = math.abs(to - from);
            if (delta.x > 1 || delta.y > 1 || delta.x + delta.y == 0 || IsBlocked(map, to))
            {
                return false;
            }

            return delta.x == 0 || delta.y == 0 ||
                    (!IsBlocked(map, new int2(from.x, to.y)) && !IsBlocked(map, new int2(to.x, from.y)));
        }

        public static bool FindPath(GridMapComponent map, int2 start, int2 end, Func<int2, bool> isOccupied, List<int2> path)
        {
            path.Clear();
            if (start.Equals(end))
            {
                return true;
            }
            if (IsBlocked(map, start) || IsBlocked(map, end))
            {
                return false;
            }

            // 序号让相同优先级的节点保持独立，不依赖 int2 的比较器。
            SortedSet<(int F, int Order, int X, int Y)> open = new();
            Dictionary<int2, int> costs = new();
            Dictionary<int2, int2> parents = new();
            HashSet<int2> closed = new();
            int order = 0;
            costs[start] = 0;
            open.Add((Heuristic(start, end), order++, start.x, start.y));

            while (open.Count > 0)
            {
                var entry = open.Min;
                open.Remove(entry);
                int2 current = new(entry.X, entry.Y);
                if (closed.Contains(current))
                {
                    continue;
                }
                if (current.Equals(end))
                {
                    for (int2 cell = end; !cell.Equals(start); cell = parents[cell])
                    {
                        path.Add(cell);
                    }
                    path.Reverse();
                    return true;
                }
                if (closed.Count >= GridConst.MaxSearchNodes)
                {
                    return false;
                }
                closed.Add(current);

                for (int dy = -1; dy <= 1; ++dy)
                {
                    for (int dx = -1; dx <= 1; ++dx)
                    {
                        int2 next = current + new int2(dx, dy);
                        if (!CanStep(map, current, next) || closed.Contains(next) || Occupied(next, end, isOccupied))
                        {
                            continue;
                        }
                        if (dx != 0 && dy != 0 &&
                            (Occupied(current + new int2(dx, 0), end, isOccupied) ||
                             Occupied(current + new int2(0, dy), end, isOccupied)))
                        {
                            continue;
                        }

                        int cost = costs[current] + (dx == 0 || dy == 0 ? 10 : 14);
                        if (costs.TryGetValue(next, out int previous) && previous <= cost)
                        {
                            continue;
                        }
                        costs[next] = cost;
                        parents[next] = current;
                        open.Add((cost + Heuristic(next, end), order++, next.x, next.y));
                    }
                }
            }
            return false;
        }

        private static bool Occupied(int2 cell, int2 end, Func<int2, bool> isOccupied)
        {
            return !cell.Equals(end) && isOccupied != null && isOccupied(cell);
        }

        private static int Heuristic(int2 from, int2 to)
        {
            int2 delta = math.abs(to - from);
            return 10 * math.max(delta.x, delta.y) + 4 * math.min(delta.x, delta.y);
        }
    }
}
