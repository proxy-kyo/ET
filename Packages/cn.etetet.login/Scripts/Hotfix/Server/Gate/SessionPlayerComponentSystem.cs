namespace ET.Server
{
    [EntitySystemOf(typeof(SessionPlayerComponent))]
    public static partial class SessionPlayerComponentSystem
    {
        [EntitySystem]
        private static void Destroy(this SessionPlayerComponent self)
        {
            Scene root = self.Root();
            if (root == null || root.IsDisposed)
            {
                return;
            }

            Player player = self.Player;
            if (player == null)
            {
                return;
            }

            if (player.GetComponent<PlayerSessionComponent>().SessionRef == self.SessionRef &&
                player.GetComponent<WaitLogoutComponent>() == null)
            {
                player.AddComponent<WaitLogoutComponent>();
                EventSystem.Instance.Publish(root, new PlayerOffline { Player = player });
            }
        }
        
        [EntitySystem]
        private static void Awake(this SessionPlayerComponent self)
        {
            self.SessionRef = self.GetParent<Session>();
        }
    }
}
