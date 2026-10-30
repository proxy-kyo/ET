using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [ComponentOf(typeof(Unit))]
    public class GridMoveComponent: Entity, IAwake, IDestroy
    {
        public int2 Cell;
        public List<int2> Path = new();
        public int Index;
        public long TimerId;
        public int StepMs;
        public int2 Target;
        public int BlockedWaits;
    }
}
