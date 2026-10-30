namespace ET.Server
{
    [Event(SceneType.Map)]
    public class UnitEnterSightRange_LegendNotifyClient: AEvent<Scene, UnitEnterSightRange>
    {
        protected override async ETTask Run(Scene scene, UnitEnterSightRange args)
        {
            Unit observer = args.A;
            UnitGateInfoComponent gate = observer.GetComponent<UnitGateInfoComponent>();
            if (gate == null)
            {
                return;
            }

            Unit unit = args.B;
            GridMoveComponent move = unit.GetComponent<GridMoveComponent>();
            LegendUnitInfo info = LegendUnitInfo.Create();
            info.UnitId = unit.Id;
            info.ConfigId = unit.ConfigId;
            info.X = move.Cell.x;
            info.Y = move.Cell.y;
            M2C_LegendUnitsAppear message = M2C_LegendUnitsAppear.Create();
            message.Units.Add(info);
            scene.Root().GetComponent<MessageSender>().Send(gate.ActorId, message);
            await ETTask.CompletedTask;
        }
    }
}
