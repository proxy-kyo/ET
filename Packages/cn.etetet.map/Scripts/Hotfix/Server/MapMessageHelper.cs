using System.Collections.Generic;

namespace ET.Server
{
    public static partial class MapMessageHelper
    {
        private static void Broadcast(Unit unit, IMessage message, bool withSelf = true)
        {
            if (message is IPool)
            {
                throw new System.Exception($"broadcast message can not be IPool: {message.GetType().FullName}");
            }

            // 纯服务端移动可在尚未加入 AOI 时调用广播。
            AOIEntity aoi = unit.GetComponent<AOIEntity>();
            if (aoi == null)
            {
                return;
            }
            Dictionary<long, EntityRef<AOIEntity>> dict = aoi.GetBeSeePlayers();
            MessageSender messageSender = unit.Root().GetComponent<MessageSender>();
            
            foreach (AOIEntity u in dict.Values)
            {
                if (!withSelf && u.Id == unit.Id)
                {
                    continue;
                }
                UnitGateInfoComponent gate = u.Unit.GetComponent<UnitGateInfoComponent>();
                if (gate == null)
                {
                    continue;
                }
                messageSender.Send(gate.ActorId, message);
            }
        }
        
        private static void SendToClient(Unit unit, IMessage message)
        {
            UnitGateInfoComponent gate = unit.GetComponent<UnitGateInfoComponent>();
            if (gate == null)
            {
                return;
            }
            MessageSender messageSender = unit.Root().GetComponent<MessageSender>();
            messageSender.Send(gate.ActorId, message);
        }
        
        public static void NoticeClient(Unit unit, IMessage message, NoticeType noticeType)
        {
            switch (noticeType)
            {
                case NoticeType.Broadcast:
                    Broadcast(unit, message);
                    break;
                case NoticeType.Self:
                    SendToClient(unit, message);
                    break;
                case NoticeType.NoNotice:
                    break;
                case NoticeType.BroadcastWithoutSelf:
                    Broadcast(unit, message, false);
                    break;
            }
        }
    }
}
