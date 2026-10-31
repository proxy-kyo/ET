namespace ET
{
    public static class GridConst
    {
        // shortcut: 原住居民多时限制 A* 开销，压测后调整。128×128 的 LegendMap1 跨半张图绕树林要 2000+ 节点。
        public const int MaxSearchNodes = 8000;

        // shortcut: 暂用固定步时，之后改读 numeric 移速。
        public const int DefaultStepMs = 400;

        public const int MaxEnterRadius = 16;
    }
}
