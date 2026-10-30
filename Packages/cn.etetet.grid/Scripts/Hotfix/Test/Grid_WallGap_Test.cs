using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Test
{
    public class Grid_WallGap_Test: ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Grid_WallGap_Test));
            GridMapComponent map = scope.TestFiber.Root.AddComponent<GridMapComponent, int, int, float>(9, 9, 1f);
            for (int y = 0; y < map.Height; ++y)
            {
                map.Blocked[y * map.Width + 4] = y != 6;
            }
            List<int2> path = new();
            int2 start = new(1, 1);
            int2 end = new(7, 1);
            if (!GridHelper.FindPath(map, start, end, null, path) || !path.Contains(new int2(4, 6)))
            {
                return 1;
            }
            int2 previous = start;
            foreach (int2 cell in path)
            {
                if (!GridHelper.CanStep(map, previous, cell))
                {
                    return 2;
                }
                previous = cell;
            }
            if (!previous.Equals(end))
            {
                return 3;
            }
            return ErrorCode.ERR_Success;
        }
    }
}
