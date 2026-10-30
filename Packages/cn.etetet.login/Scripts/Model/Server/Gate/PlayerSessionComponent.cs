namespace ET.Server
{
	[ComponentOf(typeof(Player))]
	public class PlayerSessionComponent : Entity, IAwake
	{
		public EntityRef<Session> SessionRef;

		public Session Session
		{
			get
			{
				return this.SessionRef;
			}
			set
			{
				this.SessionRef = value;
			}
		}
	}
}
