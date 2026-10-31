namespace ET
{
    public static class LegendCombatConst
    {
        // ponytail: 常量占位，接 Luban 战斗表后替换。
        public const int Damage = 25;
        public const int CooldownMs = 800;
        // 走向目标时连续这么久没挪格就放弃；距离再远，只要还在走就继续。
        public const int StuckTimeoutMs = 3000;
        public const int RespawnMs = 10000;
        public const int MonstersPerArea = 3;
    }
}
