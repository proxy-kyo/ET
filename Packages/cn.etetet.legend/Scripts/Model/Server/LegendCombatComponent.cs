namespace ET.Server
{
    [ComponentOf(typeof(Unit))]
    public class LegendCombatComponent: Entity, IAwake
    {
        public long NextAttackTime;
    }
}
