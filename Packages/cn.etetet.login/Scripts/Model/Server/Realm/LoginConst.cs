namespace ET.Server
{
    public static class LoginConst
    {
        // shortcut: 压测客户端共用 IP，正式上线前调小或改成按账号限频。
        public const int MaxLoginPerSecondPerIP = 50;
    }
}
