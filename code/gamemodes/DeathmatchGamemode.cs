namespace OpenArena;

[Library( "oa_gamemode_deathmatch" )]
public partial class DeathmatchGamemode : BaseGamemode
{
	/*
	 * TODO:
	 * - Scoring:
	 *   - Not team based
	 *   - First to 25 frags
	 * - HUD:
	 *   - If not match leader:
	 *     - Displays match leader 
	 *     - Displays you
	 *   - If match leader:
	 *     - Displays you
	 *     - Displays person below you in frags
	 * - Timer:
	 *     - Max 5 mins
	 *     - Person with most frags at end of timer wins
	 */

	public DeathmatchGamemode() : base()
	{
		TimeSinceGameStart = 0;
	}

	// - Respawning:
	//     - 3 second respawn delay
	//     - 3 second invulnerability
	protected override void CheckRespawning()
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

	// - Inventory:
	//   - Default weapon: pistol
	protected override void SetInventory( Player player )
	{
		player.Inventory.DeleteContents();
		player.Inventory.Add( new Crowbar() );
		player.Inventory.Add( new Pistol(), true );
	}
}
