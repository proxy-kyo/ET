namespace ET.Server
{
    public static partial class GateSessionKeyComponentSystem
    {
        public static void Add(this GateSessionKeyComponent self, long key, string account, long playerId)
        {
            self.sessionKey.Add(key, (account, playerId));
            self.TimeoutRemoveKey(key).Coroutine();
        }

        public static (string Account, long PlayerId) Get(this GateSessionKeyComponent self, long key)
        {
            self.sessionKey.TryGetValue(key, out var login);
            return login;
        }

        public static void Remove(this GateSessionKeyComponent self, long key)
        {
            self.sessionKey.Remove(key);
        }

        private static async ETTask TimeoutRemoveKey(this GateSessionKeyComponent self, long key)
        {
            EntityRef<GateSessionKeyComponent> selfRef = self;
            await self.Root().TimerComponent.WaitAsync(20000);
            self = selfRef;
            self.sessionKey.Remove(key);
        }
    }
}
