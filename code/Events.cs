namespace OpenArena;

public class ArenaEvent
{
	public class Player
	{
		/// <summary>
		/// Fired on client only through an RPC
		/// </summary>
		public class Kill : EventAttribute
		{
			public const string Name = "arenaevent.player.kill";

			public Kill() : base( Name ) { }
		}

		/// <summary>
		/// Fired on client only through an RPC
		/// </summary>
		public class Death : EventAttribute
		{
			public const string Name = "arenaevent.player.death";

			public Death() : base( Name ) { }
		}

		/// <summary>
		/// Fired on client only through an RPC
		/// </summary>
		public class DidDamage : EventAttribute
		{
			public const string Name = "arenaevent.player.diddamage";

			public DidDamage() : base( Name ) { }
		}
	}
}
