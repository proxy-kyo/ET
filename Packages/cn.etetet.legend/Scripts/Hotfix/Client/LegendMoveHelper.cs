using Unity.Mathematics;

namespace ET.Client
{
    public static class LegendMoveHelper
    {
        public static void MoveTo(Scene root, int2 cell)
        {
            LegendCliClientComponent client = root.GetComponent<LegendCliClientComponent>();
            if (client != null)
            {
                client.MoveSentTime = root.GetSingleton<TimeInfo>().ClientNow();
            }
            C2M_GridMoveTo message = C2M_GridMoveTo.Create();
            message.X = cell.x;
            message.Y = cell.y;
            root.GetComponent<ClientSenderComponent>().Send(message);
        }
    }
}
