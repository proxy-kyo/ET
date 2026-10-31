namespace ET.Server
{
    public static class LegendUnitHelper
    {
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
