using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;


namespace ET.Server
{
	[MessageSessionHandler(SceneType.Realm)]
	public class C2R_LoginHandler : MessageSessionHandler<C2R_Login, R2C_Login>
	{
		protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response)
		{
			response.Error = LoginValidator.Validate(request.Account, request.Password, out string name);
			if (response.Error != ErrorCode.ERR_Success)
			{
				response.Message = response.Error == ErrorCode.ERR_LoginAccountInvalid
					? "账号须为3–16位小写字母、数字或下划线" : "密码须为6–32位";
				return;
			}

			EntityRef<Session> sessionRef = session;
			// 随机分配一个Gate
			Scene root = session.Root();
			EntityRef<Scene> rootRef = root;
			if (!root.GetComponent<LoginRateLimitComponent>().TryAcquire(session.RemoteAddress.IP,
				root.GetSingleton<TimeInfo>().ServerNow()))
			{
				response.Error = ErrorCode.ERR_LoginTooFrequent;
				response.Message = "登录过于频繁";
				return;
			}
			DBComponent db = root.GetComponent<DBManagerComponent>().GetZoneDB(root.Zone());
			(int error, long playerId) = await AccountHelper.LoginOrRegister(db, name, request.Password);
			if (error != ErrorCode.ERR_Success)
			{
				response.Error = error;
				response.Message = "用户名或密码错误";
				return;
			}
			root = rootRef;
			session = sessionRef;
			ulong hash = (ulong)name.GetLongHashCode();
			
			ServiceDiscoveryProxy serviceDiscoveryProxy = root.GetComponent<ServiceDiscoveryProxy>();

			List<ServiceInfo> gates = serviceDiscoveryProxy.GetBySceneTypeAndZone(SceneType.Gate, session.Fiber().Zone);
			ServiceInfo gateServiceInfo = gates[(int)(hash % (ulong)gates.Count)];
			Log.Debug($"gate address: {gateServiceInfo.SceneName}");
			
			// 向gate请求一个key,客户端可以拿着这个key连接gate
			R2G_GetLoginKey r2GGetLoginKey = R2G_GetLoginKey.Create();
			r2GGetLoginKey.Account = name;
			r2GGetLoginKey.PlayerId = playerId;
			response.Address = gateServiceInfo.Metadata[ServiceMetaKey.InnerIPOuterPort];
			
			root = rootRef;
			MessageSender messageSender = root.GetComponent<MessageSender>();
			G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await messageSender.Call(gateServiceInfo.ActorId, r2GGetLoginKey);
			
			response.Key = g2RGetLoginKey.Key;
			response.GateId = g2RGetLoginKey.GateId;
			session = sessionRef;
			CloseSession(session).Coroutine();
		}

		private async ETTask CloseSession(Session session)
		{
			EntityRef<Session> sessionRef = session;
			await session.Root().TimerComponent.WaitAsync(1000);
			session = sessionRef;
			session?.Dispose();
		}
	}
}
