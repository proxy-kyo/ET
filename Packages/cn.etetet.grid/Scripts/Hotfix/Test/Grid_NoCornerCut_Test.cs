using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Test
{
    public class Grid_NoCornerCut_Test: ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Grid_NoCornerCut_Test));
            GridMapComponent map = scope.TestFiber.Root.AddComponent<GridMapComponent, int, int, float>(3, 3, 1f);
            List<int2> path = new();
            int2 start = new(0, 0);
            int2 end = new(1, 1);
            map.Blocked[1] = true;
            if (GridHelper.CanStep(map, start, end) ||
                !GridHelper.FindPath(map, start, end, null, path) || path.Count != 2 || !path[0].Equals(new int2(0, 1)))
            {
                return 1;
            }
            map.Blocked[1] = false;
            map.Blocked[map.Width] = true;
            if (GridHelper.CanStep(map, start, end) ||
                !GridHelper.FindPath(map, start, end, null, path) || path.Count != 2 || !path[0].Equals(new int2(1, 0)))
            {
                return 2;
            }
            map.Blocked[1] = true;
            if (GridHelper.FindPath(map, start, end, null, path) || path.Count != 0)
            {
                return 3;
            }
            return ErrorCode.ERR_Success;
        }
    }
}
