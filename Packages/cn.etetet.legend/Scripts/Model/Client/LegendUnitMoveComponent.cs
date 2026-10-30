using Unity.Mathematics;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class LegendUnitMoveComponent: Entity, IAwake
    {
        public float3 StartPos;
        public float3 EndPos;
        public long StartTime;
        public int Duration;
    }
}
