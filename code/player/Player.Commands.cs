namespace OpenArena;

partial class Player
{
	[ConCmd.Admin( "oa_godmode" )]
	public static void GodMode()
	{
		var caller = ConsoleSystem.Caller;
		var pawn = caller.Pawn;

		if ( pawn is Player player )
		{
			player.IsInvincible = !player.IsInvincible;
			Log.Trace( $"Godmode is now {( player.IsInvincible ? "ON" : "OFF" )}" );
		}
	}

	[ConCmd.Admin( "oa_give_all" )]
	public static void GiveAll()
	{
		var caller = ConsoleSystem.Caller;
		var pawn = caller.Pawn;

		if ( pawn is Player player )
		{
			player.Inventory.DeleteContents();
			player.Inventory.Add( new Pistol() );
			player.Inventory.Add( new SMG() );
			player.Inventory.Add( new RocketLauncher() );
			player.Inventory.Add( new LightningGun() );
			player.Inventory.Add( new Railgun() );
			player.Inventory.Add( new Shotgun(), true );
		}
	}
}
