using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class LegendCliClientComponent: Entity, IAwake
    {
        public int Index;
        public bool Print;
        public string Account;
        public bool Online;
        public int LoginFailed;
        public long MoveSentTime;
        public readonly List<long> MoveLatencies = new();
    }
}
