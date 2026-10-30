namespace ET.Server
{
    public static class GridMessageHelper
    {
        public static void Broadcast(Unit unit, IMessage message)
        {
            if (unit.GetComponent<AOIEntity>() == null)
            {
                return;
            }

            MessageSender sender = unit.Root().GetComponent<MessageSender>();
            foreach (AOIEntity observer in unit.GetBeSeePlayers().Values)
            {
                UnitGateInfoComponent gate = observer.Unit.GetComponent<UnitGateInfoComponent>();
                if (gate != null)
                {
                    sender.Send(gate.ActorId, message);
                }
            }
        }
    }
}
