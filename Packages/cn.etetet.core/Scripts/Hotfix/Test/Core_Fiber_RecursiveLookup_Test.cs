namespace ET.Test
{
    public class Core_Fiber_RecursiveLookup_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Core_Fiber_RecursiveLookup_Test));
            Fiber root = scope.TestFiber;
            Fiber branch = await root.CreateFiber(IdGenerater.Instance.GenerateId(), SceneType.TestEmpty, "Branch");
            Fiber nested = await branch.CreateFiber(IdGenerater.Instance.GenerateId(), SceneType.TestEmpty, "Map");
            if (root.GetFiber("Map") != null || root.GetFiber("Map", recursive: true) != nested)
            {
                return 1;
            }

            Fiber direct = await root.CreateFiber(IdGenerater.Instance.GenerateId(), SceneType.TestEmpty, "Map");
            if (root.GetFiber("Map", recursive: true) != direct)
            {
                return 2;
            }

            await using TestFiberScope other = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                $"{nameof(Core_Fiber_RecursiveLookup_Test)}_Other");
            if (other.TestFiber.GetFiber("Map", recursive: true) != null || root.GetFiber("Missing", recursive: true) != null)
            {
                return 3;
            }

            return ErrorCode.ERR_Success;
        }
    }
}
