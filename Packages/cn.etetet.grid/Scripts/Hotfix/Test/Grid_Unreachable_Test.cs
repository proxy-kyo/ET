using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Test
{
    public class Grid_Unreachable_Test: ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Grid_Unreachable_Test));
            Scene scene = scope.TestFiber.Root;
            GridMapComponent map = scene.AddComponent<GridMapComponent, int, int, float>(7, 7, 1f);
            for (int y = 2; y <= 4; ++y)
            {
                for (int x = 2; x <= 4; ++x)
                {
                    map.Blocked[y * map.Width + x] = x != 3 || y != 3;
                }
            }
            List<int2> path = new() { new int2(6, 6) };
            if (GridHelper.FindPath(map, new int2(0, 0), new int2(3, 3), null, path) || path.Count != 0)
            {
                return 1;
            }
            if (GridHelper.FindPath(map, new int2(0, 0), new int2(-1, 0), null, path) || path.Count != 0)
            {
                return 2;
            }
            scene.RemoveComponent<GridMapComponent>();
            map = scene.AddComponent<GridMapComponent, int, int, float>(GridConst.MaxSearchNodes + 2, 1, 1f);
            path.Add(new int2(1, 0));
            if (GridHelper.FindPath(map, new int2(0, 0), new int2(map.Width - 1, 0), null, path) || path.Count != 0)
            {
                return 3;
            }
            return ErrorCode.ERR_Success;
        }
    }
}
