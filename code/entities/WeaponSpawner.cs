namespace OpenArena;

[Title( "Weapon Spawner" ), Icon( "add" ), Category( "World" )]
[EditorModel( "models/world/weapon_spawn.vmdl" )]
[Library( "oa_info_weapon_spawn" )]
[HammerEntity]
public partial class WeaponSpawner : BasePickup
{
	[Property, Net] public string WeaponLibraryName { get; set; } = "oa_weapon_smg";
	private WeaponDataAsset WeaponData { get; set; }

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		WeaponData = WeaponDataAsset.FindByLibraryName( WeaponLibraryName );
		SetGhostModel( WeaponData.WorldModel );
	}

	public override void Spawn()
	{
		base.Spawn();
		WeaponData = WeaponDataAsset.FindByLibraryName( WeaponLibraryName );
	}

	public override void OnPickup( Player player )
	{
		base.OnPickup( player );
		GivePlayerWeapon( player );
	}

	private void GivePlayerWeapon( Player player )
	{
		if ( !IsServer )
			return;

		PlayDeploySound( To.Single( player ) );

		if ( player.Inventory.ContainsAny( WeaponLibraryName ) )
		{
			player.Inventory.First( WeaponLibraryName ).Ammo.Count += 100;
			return;
		}

		var weapon = TypeLibrary.Create<BaseWeapon>( WeaponLibraryName );
		player.Inventory.Add( weapon, true );
	}

	[ClientRpc]
	public void PlayDeploySound()
	{
		Sound.FromScreen( "deploy" );
	}
}
