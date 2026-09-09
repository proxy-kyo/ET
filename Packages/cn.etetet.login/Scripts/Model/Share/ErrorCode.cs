namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_ConnectGateKeyError = ErrorCode.ERR_WithException + PackageType.Login * 1000 + 1; // 100009001
        public const int ERR_LoginAccountPasswordError = ErrorCode.ERR_WithException + PackageType.Login * 1000 + 2;
    }
}