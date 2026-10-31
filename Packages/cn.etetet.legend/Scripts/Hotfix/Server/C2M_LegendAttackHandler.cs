namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_LegendAttackHandler: MessageLocationHandler<Unit, C2M_LegendAttack, M2C_LegendAttack>
    {
        protected override async ETTask Run(Unit unit, C2M_LegendAttack request, M2C_LegendAttack response)
        {
            response.Error = LegendCombatHelper.Attack(unit, request.TargetUnitId);
            await ETTask.CompletedTask;
        }
    }
}
