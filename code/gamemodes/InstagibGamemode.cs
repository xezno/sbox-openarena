namespace OpenArena;

[Library( "oa_gamemode_instagib" )]
public class InstagibGamemode : BaseGamemode
{
	/*
	 * TODO:
	 * - No weapon spawns
	 */

	protected override void SetInventory( Player player )
	{
		player.Inventory.DeleteContents();
		player.Inventory.Add( new Railgun(), true );
	}

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
}
