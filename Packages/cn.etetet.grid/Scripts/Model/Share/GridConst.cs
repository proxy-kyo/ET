namespace ET
{
    public static class GridConst
    {
        // shortcut: 原住居民多时限制 A* 开销，压测后调整。
        public const int MaxSearchNodes = 2000;

        // shortcut: 暂用固定步时，之后改读 numeric 移速。
        public const int DefaultStepMs = 400;

        public const int MaxEnterRadius = 16;
    }
}
