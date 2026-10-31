namespace ET.Server
{
    [Event(SceneType.Gate)]
    public class PlayerOffline_LegendLeaveMap: AEvent<Scene, PlayerOffline>
    {
        protected override ETTask Run(Scene root, PlayerOffline args)
        {
            Player player = args.Player;
            LegendPlayerMapComponent playerMap = player.GetComponent<LegendPlayerMapComponent>();
            MessageSender sender = root.GetComponent<MessageSender>();
            if (playerMap == null || sender == null)
            {
                return ETTask.CompletedTask;
            }

            G2M_LegendLeaveMap message = G2M_LegendLeaveMap.Create();
            message.PlayerId = player.Id;
            sender.Send(playerMap.MapActorId, message);
            player.RemoveComponent<LegendPlayerMapComponent>();
            return ETTask.CompletedTask;
        }
    }
}
