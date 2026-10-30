using System.Collections.Generic;

namespace ET.Server
{
    [ChildOf(typeof(DBComponent))]
    public class Account : Entity, IAwake
    {
        public string Name;
        public byte[] PasswordHash;
        public byte[] Salt;
        public List<long> CharacterIds = new();
        public long CreateTime;
    }
}
