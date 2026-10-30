using Unity.Mathematics;

namespace ET.Client
{
    [MessageHandler(SceneType.Current)]
    public class M2C_LegendUnitsAppearHandler: MessageHandler<Scene, M2C_LegendUnitsAppear>
    {
        protected override async ETTask Run(Scene scene, M2C_LegendUnitsAppear message)
        {
            UnitComponent units = scene.GetComponent<UnitComponent>();
            GridMapComponent map = scene.GetComponent<GridMapComponent>();
            foreach (LegendUnitInfo info in message.Units)
            {
                if (units.Get(info.UnitId) != null)
                {
                    continue;
                }
                Unit unit = units.AddChildWithId<Unit, int>(info.UnitId, info.ConfigId);
                units.Add(unit);
                unit.Position = GridHelper.CellToWorld(map, new int2(info.X, info.Y));
                LegendUnitMoveComponent move = unit.AddComponent<LegendUnitMoveComponent>();
                move.StartPos = unit.Position;
                move.EndPos = unit.Position;
                EventSystem.Instance.Publish(scene, new LegendUnitAppear { Unit = unit });
            }
            await ETTask.CompletedTask;
        }
    }
}
