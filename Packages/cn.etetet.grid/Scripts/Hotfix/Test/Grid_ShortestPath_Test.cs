using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Test
{
    public class Grid_ShortestPath_Test: ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Grid_ShortestPath_Test));
            GridMapComponent map = scope.TestFiber.Root.AddComponent<GridMapComponent, int, int, float>(8, 8, 2f);
            List<int2> path = new();
            if (!GridHelper.FindPath(map, new int2(1, 1), new int2(6, 1), null, path) || path.Count != 5 ||
                !path[0].Equals(new int2(2, 1)) || !path[path.Count - 1].Equals(new int2(6, 1)))
            {
                return 1;
            }
            if (!GridHelper.FindPath(map, new int2(1, 1), new int2(6, 6), null, path) || path.Count != 5 ||
                !path[0].Equals(new int2(2, 2)) || !path[path.Count - 1].Equals(new int2(6, 6)))
            {
                return 2;
            }
            if (!GridHelper.FindPath(map, new int2(1, 1), new int2(1, 1), null, path) || path.Count != 0)
            {
                return 3;
            }
            int2 cell = new(3, 4);
            if (!GridHelper.CellToWorld(map, cell).Equals(new float3(7, 0, 9)) ||
                !GridHelper.WorldToCell(map, new float3(7.9f, 15, 9.9f)).Equals(cell) ||
                !GridHelper.WorldToCell(map, new float3(-0.1f, 0, -0.1f)).Equals(new int2(-1, -1)))
            {
                return 4;
            }
            if (!GridHelper.IsBlocked(map, new int2(-1, 0)) || !GridHelper.IsBlocked(map, new int2(8, 0)) ||
                GridHelper.CanStep(map, cell, cell) || GridHelper.CanStep(map, cell, cell + new int2(2, 0)) ||
                GridHelper.Key(new int2(-1, 0)) == GridHelper.Key(new int2(0, -1)))
            {
                return 5;
            }
            return ErrorCode.ERR_Success;
        }
    }
}
