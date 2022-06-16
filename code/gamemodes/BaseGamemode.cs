
namespace OpenArena;

[Library( "oa_gamemode_deathmatch" )]
public partial class BaseGamemode : BaseNetworkable
{
	[ConVar.Replicated( "oa_debug_gamemode" )] public static bool Debug { get; set; }
	[Net, Predicted] protected TimeSince TimeSinceGameStart { get; set; }
	protected List<Player> Players => Entity.All.OfType<Player>().ToList();

	public BaseGamemode()
	{
		Event.Register( this );
	}

	public virtual void RespawnPlayer( Player player )
	{
		Log.Trace( $"Gamemode: Respawning {player}" );

		player.Respawn();
		SetInventory( player );
		MoveToSpawnpoint( player );
	}

	public virtual void Simulate()
	{
		if ( Debug )
		{
			var pos = new Vector2( 350, 450 );
			if ( Host.IsServer )
				pos.y += 100;

			var realm = Host.IsServer ? "Server" : "Client";

			DebugOverlay.ScreenText( $"[GAMEMODE]\n" +
				$"Realm:                       {realm}\n" +
				$"TimeSinceGameStart:          {TimeSinceGameStart}\n" +
				$"Type:                        {GetType().Name}",
				pos );
		}

		CheckRespawning();
	}

	protected virtual void CheckRespawning()
	{
	}

	protected virtual void SetInventory( Player player )
	{
	}

	protected virtual void MoveToSpawnpoint( Entity pawn )
	{
		var spawnpoint = Entity.All
								.OfType<SpawnPoint>()               // get all SpawnPoint entities
								.OrderBy( x => Guid.NewGuid() )     // order them by random
								.FirstOrDefault();                  // take the first one

		if ( spawnpoint == null )
		{
			Log.Warning( $"Couldn't find spawnpoint for {pawn}!" );
			return;
		}

		pawn.Transform = spawnpoint.Transform;
	}
}
