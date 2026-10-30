namespace ET.Server
{
    [EntitySystemOf(typeof(GridMoveComponent))]
    public static partial class GridMoveComponentSystem
    {
        [EntitySystem]
        private static void Awake(this GridMoveComponent self)
        {
            self.Cell = default;
            self.Path.Clear();
            self.Index = 0;
            self.TimerId = 0;
            self.StepMs = GridConst.DefaultStepMs;
            self.Target = default;
            self.BlockedWaits = 0;
        }

        [EntitySystem]
        private static void Destroy(this GridMoveComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            Scene scene = unit.Scene();
            scene.TimerComponent?.Remove(ref self.TimerId);
            GridOccupancyComponent occupancy = scene.GetComponent<GridOccupancyComponent>();
            // Scene 销毁时占用组件可能已先释放。
            if (occupancy != null && occupancy.Occupants.TryGetValue(GridHelper.Key(self.Cell), out long owner) && owner == unit.Id)
            {
                occupancy.Occupants.Remove(GridHelper.Key(self.Cell));
            }
            self.Path.Clear();
        }
    }
}
