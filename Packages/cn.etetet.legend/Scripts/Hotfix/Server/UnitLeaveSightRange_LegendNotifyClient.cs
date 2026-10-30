namespace ET.Server
{
    [Event(SceneType.LegendMap)]
    public class UnitLeaveSightRange_LegendNotifyClient: AEvent<Scene, UnitLeaveSightRange>
    {
        protected override async ETTask Run(Scene scene, UnitLeaveSightRange args)
        {
            Unit observer = args.A;
            Unit unit = args.B;
            if (observer.Id == unit.Id)
            {
                return;
            }
            UnitGateInfoComponent gate = observer.GetComponent<UnitGateInfoComponent>();
            if (gate == null)
            {
                return;
            }

            M2C_LegendUnitsDisappear message = M2C_LegendUnitsDisappear.Create();
            message.UnitIds.Add(unit.Id);
            scene.Root().GetComponent<MessageSender>().Send(gate.ActorId, message);
            await ETTask.CompletedTask;
        }
    }
}
