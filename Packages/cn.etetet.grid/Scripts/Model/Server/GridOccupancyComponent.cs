using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class GridOccupancyComponent: Entity, IAwake
    {
        public Dictionary<long, long> Occupants = new();
    }
}
