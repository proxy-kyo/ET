namespace ET
{
    [EntitySystemOf(typeof(GridMapComponent))]
    public static partial class GridMapComponentSystem
    {
        [EntitySystem]
        private static void Awake(this GridMapComponent self, int width, int height, float cellSize)
        {
            self.Width = width;
            self.Height = height;
            self.CellSize = cellSize;
            self.Blocked = new bool[width * height];
        }
    }
}
