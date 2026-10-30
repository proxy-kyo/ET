namespace ET.Server
{
    [EntitySystemOf(typeof(GridOccupancyComponent))]
    public static partial class GridOccupancyComponentSystem
    {
        [EntitySystem]
        private static void Awake(this GridOccupancyComponent self)
        {
            self.Occupants.Clear();
        }
    }
}
