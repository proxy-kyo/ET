using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Test
{
    public class Grid_OccupiedPath_Test: ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Grid_OccupiedPath_Test));
            GridMapComponent map = scope.TestFiber.Root.AddComponent<GridMapComponent, int, int, float>(7, 5, 1f);
            int2 occupied = new(3, 2);
            int2 start = new(1, 2);
            int2 end = new(5, 2);
            List<int2> path = new();
            if (!GridHelper.FindPath(map, start, end, cell => cell.Equals(occupied), path) || path.Contains(occupied))
            {
                return 1;
            }
            if (!GridHelper.FindPath(map, start, end, cell => cell.Equals(occupied) || cell.Equals(end), path) ||
                path.Contains(occupied) || !path[path.Count - 1].Equals(end))
            {
                return 2;
            }
            if (!GridHelper.FindPath(map, new int2(0, 0), new int2(1, 1), cell => cell.Equals(new int2(1, 0)), path) ||
                path.Count != 2 || !path[0].Equals(new int2(0, 1)))
            {
                return 3;
            }
            return ErrorCode.ERR_Success;
        }
    }
}
