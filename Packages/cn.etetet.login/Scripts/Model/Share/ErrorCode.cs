namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_ConnectGateKeyError = ErrorCode.ERR_WithException + PackageType.Login * 1000 + 1; // 100009001
        public const int ERR_LoginAccountPasswordError = ErrorCode.ERR_WithException + PackageType.Login * 1000 + 2;
        public const int ERR_LoginAccountInvalid = ErrorCode.ERR_WithException + PackageType.Login * 1000 + 3;
        public const int ERR_LoginPasswordInvalid = ErrorCode.ERR_WithException + PackageType.Login * 1000 + 4;
        public const int ERR_LoginTooFrequent = ErrorCode.ERR_WithException + PackageType.Login * 1000 + 5;
    }
}
