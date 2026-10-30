using Unity.Mathematics;

namespace ET.Client
{
    public static class LegendUnitMoveHelper
    {
        public static float3 Position(LegendUnitMoveComponent move, long now)
        {
            return move.Duration <= 0 ? move.EndPos : math.lerp(move.StartPos, move.EndPos,
                math.saturate((float)(now - move.StartTime) / move.Duration));
        }
    }
}
