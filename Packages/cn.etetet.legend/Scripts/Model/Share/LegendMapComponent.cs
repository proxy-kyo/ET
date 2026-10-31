using Unity.Mathematics;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class LegendMapComponent: Entity, IAwake
    {
        public int2 Spawn;
        public int[] CellColors;
    }
}
