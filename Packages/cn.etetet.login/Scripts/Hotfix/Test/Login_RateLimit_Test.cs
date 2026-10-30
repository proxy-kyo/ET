using ET.Server;

namespace ET.Test
{
    public class Login_RateLimit_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Login_RateLimit_Test));
            LoginRateLimitComponent limiter = scope.TestFiber.Root.AddComponent<LoginRateLimitComponent>();
            for (int i = 0; i < LoginConst.MaxLoginPerSecondPerIP; ++i)
            {
                if (!limiter.TryAcquire("127.0.0.1", 1000)) return 1;
            }
            if (limiter.TryAcquire("127.0.0.1", 1000)) return 2;
            if (!limiter.TryAcquire("127.0.0.2", 1000)) return 3;
            if (!limiter.TryAcquire("127.0.0.1", 1020)) return 4;
            if (limiter.TryAcquire("127.0.0.1", 1020)) return 5;
            if (!limiter.TryAcquire("127.0.0.1", 2020)) return 6;
            return ErrorCode.ERR_Success;
        }
    }
}
