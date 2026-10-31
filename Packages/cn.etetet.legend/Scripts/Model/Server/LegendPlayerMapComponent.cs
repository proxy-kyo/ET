namespace ET.Server
{
    [ComponentOf(typeof(Player))]
    public class LegendPlayerMapComponent: Entity, IAwake
    {
        public ActorId MapActorId;
    }
}
