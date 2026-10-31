namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_LegendCliCommand = PackageType.Legend * 1000 + 1;
        public const int ERR_LegendMapNotFound = ErrorCode.ERR_WithException + PackageType.Legend * 1000 + 1;
    }
}
