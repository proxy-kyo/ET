namespace ET.Server
{
    [Invoke(TimerInvokeType.GridStep)]
    public class GridStepTimer: ATimer<GridMoveComponent>
    {
        protected override void Run(GridMoveComponent self)
        {
            self.TimerId = 0;
            GridMoveHelper.Step(self.GetParent<Unit>());
        }
    }
}
