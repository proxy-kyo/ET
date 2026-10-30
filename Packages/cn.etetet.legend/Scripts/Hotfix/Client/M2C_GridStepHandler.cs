using Unity.Mathematics;

namespace ET.Client
{
    [MessageHandler(SceneType.Current)]
    public class M2C_GridStepHandler: MessageHandler<Scene, M2C_GridStep>
    {
        protected override async ETTask Run(Scene scene, M2C_GridStep message)
        {
            Unit unit = scene.GetComponent<UnitComponent>().Get(message.UnitId);
            // AOI 离开通知后，仍可能收到已经在途的步进消息。
            if (unit == null)
            {
                return;
            }
            LegendUnitMoveComponent move = unit.GetComponent<LegendUnitMoveComponent>();
            long now = scene.GetSingleton<TimeInfo>().ClientNow();
            move.StartPos = LegendUnitMoveHelper.Position(move, now);
            move.EndPos = GridHelper.CellToWorld(scene.GetComponent<GridMapComponent>(), new int2(message.X, message.Y));
            move.StartTime = now;
            move.Duration = message.DurationMs;
            unit.Position = move.EndPos;
            await ETTask.CompletedTask;
        }
    }
}
