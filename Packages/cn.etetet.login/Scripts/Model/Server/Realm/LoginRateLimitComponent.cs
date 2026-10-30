using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class LoginRateLimitComponent : Entity, IAwake
    {
        public readonly Dictionary<string, (double Tokens, long Time)> Buckets = new();
    }
}
