namespace ET
{
    public static partial class SceneType
    {
        // 独立场景类型，避免继承其他包的 Invoke/Event 处理器。
        public const int Legend = PackageType.Legend * 1000 + 1;
        public const int LegendClient = PackageType.Legend * 1000 + 2;
        public const int LegendMap = PackageType.Legend * 1000 + 3;
    }
}
