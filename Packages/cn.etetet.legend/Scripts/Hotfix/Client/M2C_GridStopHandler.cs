using Unity.Mathematics;

namespace ET.Client
{
    [MessageHandler(SceneType.Current)]
    public class M2C_GridStopHandler: MessageHandler<Scene, M2C_GridStop>
    {
        protected override async ETTask Run(Scene scene, M2C_GridStop message)
        {
            Unit unit = scene.GetComponent<UnitComponent>().Get(message.UnitId);
            if (unit == null)
            {
                return;
            }
            LegendUnitMoveComponent move = unit.GetComponent<LegendUnitMoveComponent>();
            move.EndPos = GridHelper.CellToWorld(scene.GetComponent<GridMapComponent>(), new int2(message.X, message.Y));
            move.StartPos = move.EndPos;
            move.Duration = 0;
            unit.Position = move.EndPos;
            await ETTask.CompletedTask;
        }
    }
}
