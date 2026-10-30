namespace ET.Server
{
    public static class CharacterIdHelper
    {
        public static long New()
        {
            // shortcut: 随机 Id 的碰撞由 MongoDB 唯一索引拒绝，插入方重试；扩大规模时仍须保留该约束。
            long id;
            do
            {
                id = RandomGenerator.RandInt64() & long.MaxValue;
            } while (id == 0);
            return id;
        }
    }
}
