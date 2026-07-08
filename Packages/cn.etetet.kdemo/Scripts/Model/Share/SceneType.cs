namespace ET
{
    public static partial class SceneType
    {
        // 客户端
        public const int KDemo = PackageType.KDemo * 1000 + 1;

        // KDemo 自己的客户端子纤程类型，不复用 statesync 的 SceneType.Client，
        // 避免继承 statesync 的 FiberInit_Client → AppStartInitFinish → YIUI 登录流程。
        public const int KDemoClient = PackageType.KDemo * 1000 + 2;
    }
}
