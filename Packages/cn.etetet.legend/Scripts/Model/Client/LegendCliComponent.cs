using System.Collections.Generic;
using System.Threading;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class LegendCliComponent: Entity, IAwake, IDestroy
    {
        public readonly List<LegendCliClient> Clients = new();
        public int CurrentIndex;
        public int NextIndex;
        public int SpawnIndex;
        public CancellationTokenSource CancellationTokenSource;
    }
}
