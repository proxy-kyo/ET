namespace ET.Server
{
    public static class LegendUnitHelper
    {
        public static async ETTask RemovePlayer(Unit unit)
        {
            EntityRef<Unit> unitRef = unit;
            unit.RemoveComponent<AOIEntity>();
            await unit.RemoveLocation(LocationType.Unit);
            unit = unitRef;
            unit.GetParent<UnitComponent>().Remove(unit.Id);
        }

        public static void InitNumeric(Unit unit)
        {
            NumericComponent numeric = unit.AddComponent<NumericComponent>();
            foreach ((NumericType k, long v) in unit.Config().KV)
            {
                numeric.SetNoEvent(k, v);
            }
            numeric.SetNoEvent(NumericType.AOI, LegendMapConst.AOIRange);
            numeric.SetNoEvent(NumericType.Phase, (long)PhaseType.Normal);
        }
    }
}
