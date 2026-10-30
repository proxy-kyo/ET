using System.Net.Sockets;
using ET.Server;

namespace ET.Test
{
    public class Login_SessionOwnership_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Login_SessionOwnership_Test));
            Scene root = scope.TestFiber.Root;
            NetComponent net = root.AddComponent<NetComponent, IKcpTransport>(new UdpTransport(AddressFamily.InterNetwork));
            Player player = root.AddComponent<PlayerComponent>().AddChildWithId<Player, string>(CharacterIdHelper.New(), "ownership");
            PlayerSessionComponent owner = player.AddComponent<PlayerSessionComponent>();
            Session oldSession = net.AddChildWithId<Session, AService>(1, net.AService);
            oldSession.AddComponent<SessionPlayerComponent>().Player = player;
            owner.Session = oldSession;
            Session newSession = net.AddChildWithId<Session, AService>(2, net.AService);
            newSession.AddComponent<SessionPlayerComponent>().Player = player;
            owner.Session = newSession;
            oldSession.Dispose();
            if (player.GetComponent<WaitLogoutComponent>() != null || owner.Session != newSession) return 1;
            newSession.Dispose();
            if (player.GetComponent<WaitLogoutComponent>() == null) return 2;
            // A second destroy notification for the same owner must not add the component twice.
            Session lastSession = net.AddChildWithId<Session, AService>(3, net.AService);
            owner.Session = lastSession;
            lastSession.AddComponent<SessionPlayerComponent>().Player = player;
            lastSession.Dispose();
            return player.GetComponent<WaitLogoutComponent>() != null ? ErrorCode.ERR_Success : 3;
        }
    }
}
