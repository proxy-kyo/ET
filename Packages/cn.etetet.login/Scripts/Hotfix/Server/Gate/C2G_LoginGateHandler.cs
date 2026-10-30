using System;


namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_LoginGateHandler : MessageSessionHandler<C2G_LoginGate, G2C_LoginGate>
    {
        protected override async ETTask Run(Session session, C2G_LoginGate request, G2C_LoginGate response)
        {
            Scene root = session.Root();
            (string account, long playerId) = root.GetComponent<GateSessionKeyComponent>().Get(request.Key);
            if (account == null)
            {
                response.Error = ErrorCode.ERR_ConnectGateKeyError;
                response.Message = "Gate key验证失败!";
                return;
            }
            
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            PlayerComponent playerComponent = root.GetComponent<PlayerComponent>();
            Player player = playerComponent.GetByAccount(account);
            EntityRef<Session> sessionRef = session;
            
            if (player == null)
            {
                player = playerComponent.AddChildWithId<Player, string>(playerId, account);
                EntityRef<Player> playerRef = player;
                playerComponent.Add(player);
                PlayerSessionComponent playerSessionComponent = player.AddComponent<PlayerSessionComponent>();
                playerSessionComponent.AddComponent<MailBoxComponent, int>(MailBoxType.GateSession);
                
                player = playerRef;
                session = sessionRef;
                session.AddComponent<SessionPlayerComponent>().Player = player;
                playerSessionComponent.Session = session;
            }
            else
            {
                Session oldSession = player.GetComponent<PlayerSessionComponent>().Session;
                if (oldSession != null && oldSession != session)
                {
                    G2C_KickOut kickOut = G2C_KickOut.Create();
                    kickOut.Message = "账号已在其他连接登录";
                    oldSession.Send(kickOut);
                    CloseSession(oldSession).Coroutine();
                }
                player.RemoveComponent<WaitLogoutComponent>();
                session.AddComponent<SessionPlayerComponent>().Player = player;
                player.GetComponent<PlayerSessionComponent>().Session = session;
            }

            response.PlayerId = player.Id;
            await ETTask.CompletedTask;
        }

        private static async ETTask CloseSession(Session session)
        {
            EntityRef<Session> sessionRef = session;
            await session.Root().TimerComponent.WaitAsync(1000);
            session = sessionRef;
            session?.Dispose();
        }
    }
}
