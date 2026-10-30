namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class GridMapComponent: Entity, IAwake<int, int, float>
    {
        public int Width;
        public int Height;
        public float CellSize;
        public bool[] Blocked;
    }
}
