using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Test
{
    public class Legend_TiledMap_Test: ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Legend_TiledMap_Test));
            Scene scene = scope.TestFiber.Root;
            LegendMapHelper.Load(scene, LegendMapName.First);
            GridMapComponent grid = scene.GetComponent<GridMapComponent>();
            LegendMapComponent map = scene.GetComponent<LegendMapComponent>();
            if (grid.Width != 128 || grid.Height != 128)
            {
                return 1;
            }
            if (!map.Spawn.Equals(new int2(64, 64)))
            {
                return 2;
            }
            if (GridHelper.IsBlocked(grid, map.Spawn))
            {
                return 3;
            }
            int blocked = 0;
            foreach (bool cell in grid.Blocked)
            {
                if (cell) ++blocked;
            }
            if (blocked == 0)
            {
                return 4;
            }
            if (!GridHelper.IsBlocked(grid, new int2(52, 60)))
            {
                return 5;
            }
            if (GridHelper.IsBlocked(grid, new int2(64, 52)))
            {
                return 6;
            }
            List<int2> path = new();
            if (!GridHelper.FindPath(grid, map.Spawn, new int2(84, 64), null, path))
            {
                return 7;
            }
            return ErrorCode.ERR_Success;
        }
    }
}
