using ET.Server;

namespace ET.Test
{
    public class Aoi_HasClient_Test: ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Aoi_HasClient_Test));
            Scene scene = scope.TestFiber.Root;
            scene.AddComponent<AOIManagerComponent>();
            UnitComponent units = scene.AddComponent<UnitComponent>();
            Unit a = Create(units, true);
            Unit b = Create(units, false);
            AOIEntity aoiA = a.GetComponent<AOIEntity>();
            AOIEntity aoiB = b.GetComponent<AOIEntity>();
            if (!aoiA.HasClient || aoiB.HasClient)
            {
                return 1;
            }
            if (!aoiA.SeeUnits.ContainsKey(b.Id) || !aoiB.BeSeePlayers.ContainsKey(a.Id))
            {
                return 2;
            }
            if (!aoiB.SeeUnits.ContainsKey(a.Id) || aoiA.BeSeePlayers.ContainsKey(b.Id))
            {
                return 3;
            }
            if (aoiA.SeePlayers.ContainsKey(b.Id) || !aoiB.SeePlayers.ContainsKey(a.Id))
            {
                return 4;
            }
            Unit c = Create(units, false);
            AOIEntity aoiC = c.GetComponent<AOIEntity>();
            if (!aoiB.SeeUnits.ContainsKey(c.Id) || !aoiC.SeeUnits.ContainsKey(b.Id) ||
                aoiB.SeePlayers.ContainsKey(c.Id) || aoiC.SeePlayers.ContainsKey(b.Id) ||
                aoiB.BeSeePlayers.ContainsKey(c.Id) || aoiC.BeSeePlayers.ContainsKey(b.Id))
            {
                return 5;
            }
            aoiA.LeaveSight(aoiB);
            aoiB.LeaveSight(aoiA);
            if (aoiA.SeeUnits.ContainsKey(b.Id) || aoiB.BeSeePlayers.ContainsKey(a.Id) ||
                aoiB.SeeUnits.ContainsKey(a.Id) || aoiB.SeePlayers.ContainsKey(a.Id) ||
                aoiA.BeSeeUnits.ContainsKey(b.Id))
            {
                return 6;
            }
            return ErrorCode.ERR_Success;
        }

        private static Unit Create(UnitComponent units, bool hasClient)
        {
            Unit unit = units.AddChild<Unit, int>(1);
            unit.UnitType = UnitType.Player;
            units.Add(unit);
            NumericComponent numeric = unit.AddComponent<NumericComponent>();
            numeric.SetNoEvent(NumericType.AOI, AOIManagerComponent.CellSize);
            numeric.SetNoEvent(NumericType.Phase, (long)PhaseType.Normal);
            if (hasClient)
            {
                unit.AddComponent<UnitGateInfoComponent>();
            }
            unit.AddComponent<AOIEntity>();
            return unit;
        }
    }
}
