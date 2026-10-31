namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_LegendCliCommand = PackageType.Legend * 1000 + 1;
        // 动作失败由调用方处理，放在 ERR_WithoutException 以上，RPC 不抛异常。
        public const int ERR_LegendAttackTargetNotFound = ErrorCode.ERR_WithoutException + PackageType.Legend * 1000 + 2;
        public const int ERR_LegendAttackNotAdjacent = ErrorCode.ERR_WithoutException + PackageType.Legend * 1000 + 3;
        public const int ERR_LegendAttackCooldown = ErrorCode.ERR_WithoutException + PackageType.Legend * 1000 + 4;
        public const int ERR_LegendAttackSelf = ErrorCode.ERR_WithoutException + PackageType.Legend * 1000 + 5;
        public const int ERR_LegendAttackCancelled = ErrorCode.ERR_WithoutException + PackageType.Legend * 1000 + 6;
        public const int ERR_LegendMapNotFound = ErrorCode.ERR_WithException + PackageType.Legend * 1000 + 1;
    }
}
