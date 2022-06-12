namespace OpenArena;

[Library( "oa_gamemode_deathmatch" )]
public partial class DeathmatchGamemode : BaseNetworkable
{
	protected List<Player> Players => Entity.All.OfType<Player>().ToList();

	// - Inventory:
	//   - Default weapon: pistol
	private void SetInventory( Player player )
	{
		player.Inventory.DeleteContents();
		player.Inventory.Add( new Pistol(), true );
	}

	public void RespawnPlayer( Player player )
	{
		Log.Trace( $"DeathmatchGamemode: Respawning {player}" );

		player.Respawn();
		SetInventory( player );
		MoveToSpawnpoint( player );
	}

	private void MoveToSpawnpoint( Entity pawn )
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

	// - Scoring:
	//   - Not team based
	//   - First to 25 frags

	// - HUD:
	//   - If not match leader:
	//     - Displays match leader 
	//     - Displays you
	//   - If match leader:
	//     - Displays you
	//     - Displays person below you in frags

	// - Timer:
	//     - Max 5 mins
	//     - Person with most frags at end of timer wins

	// - Respawning:
	//     - 3 second respawn delay
	//     - 3 second invulnerability
	public void Simulate()
	{
		foreach ( var player in Players )
		{
			if ( player.LifeState == LifeState.Dead )
			{
				if ( player.TimeSinceDied > 3 && Host.IsServer )
				{
					RespawnPlayer( player );
				}
			}
		}
	}
}
