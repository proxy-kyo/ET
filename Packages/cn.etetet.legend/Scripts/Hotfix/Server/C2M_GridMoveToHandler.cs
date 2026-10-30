using Unity.Mathematics;

namespace ET.Server
{
    [MessageHandler(SceneType.LegendMap)]
    public class C2M_GridMoveToHandler: MessageLocationHandler<Unit, C2M_GridMoveTo>
    {
        protected override async ETTask Run(Unit unit, C2M_GridMoveTo message)
        {
            // ponytail: 暂不回移动错误提示，后续接 M2C 错误消息。
            GridMoveHelper.MoveTo(unit, new int2(message.X, message.Y));
            await ETTask.CompletedTask;
        }
    }
}
